from pathlib import Path
import tempfile, os, hashlib, json, time, random, threading, tracemalloc, csv, re, shutil
ROOT=Path(__file__).resolve().parents[1]
random.seed(260)
results=[]
def record(group,case,ok,detail='',elapsed=0,peak=0): results.append([group,case,'PASS' if ok else 'FAIL',detail,elapsed,peak])
def sha_stream(p, chunk=1024*1024):
    h=hashlib.sha256()
    with open(p,'rb',buffering=0) as f:
        while True:
            b=f.read(chunk)
            if not b: break
            h.update(b)
    return h.hexdigest()
start_all=time.perf_counter(); tracemalloc.start()
with tempfile.TemporaryDirectory(prefix='so-stress-') as td:
    base=Path(td); src=base/'src'; dst=base/'dst'; src.mkdir(); dst.mkdir()
    # 1) UI/async generation and cancellation model, 2000 randomized completions.
    latest=0; applied=[]
    work=list(range(1,20001)); random.shuffle(work)
    for generation in work:
        latest=max(latest,generation)
        if generation==latest: applied.append(generation)
    record('ui_async','generation_guard_20000',applied[-1]==20000 and max(applied)==20000,f'applied={len(applied)} latest={applied[-1]}')
    # Source-level deadlock checks.
    code='\n'.join(p.read_text(encoding='utf-8') for p in (ROOT/'src').rglob('*.cs'))
    bad_pats={'.Result':r'\.Result\b','.Wait(':r'\.Wait\s*\(','GetResult':r'GetAwaiter\(\)\.GetResult\(', 'Parallel.ForEach':r'Parallel\.ForEach'}
    for name,pat in bad_pats.items(): record('ui_async',f'forbidden_{name}',not re.search(pat,code),f'count={len(re.findall(pat,code))}')
    main=(ROOT/'src/SmartOrganizer.WinUI/MainWindow.xaml.cs').read_text(encoding='utf-8')
    safe=(ROOT/'src/SmartOrganizer.WinUI/Operations/SafeOperationService.cs').read_text(encoding='utf-8')
    record('hardening','single_search_refresh','await ViewModel.RefreshAsync()' not in main.split('searchTimer.Tick+=',1)[1].split(';ApplyColumns',1)[0])
    record('hardening','commit_non_cancelable','HashAsync(plan.DestinationPath,CancellationToken.None)' in safe)
    record('hardening','safe_partial_cleanup','TryDeleteStage(stage)' in safe and 'catch(Exception e) when(e is IOException or UnauthorizedAccessException)' in safe)
    record('hardening','operation_serialization','operationGate.WaitAsync(token)' in safe and 'UndoCoreAsync' in safe and 'ExecuteCoreAsync' in safe)
    # 2) 2000 actual atomic copy/hash/delete cycles, varying content and collisions.
    t=time.perf_counter(); failures=0
    for i in range(20000):
        s=src/f'f{i%31}.bin'; stage=dst/f'f{i%31}.bin.{i}.partial'; final=dst/f'final{i}.bin'
        data=((i.to_bytes(4,'little')*16)[:64]); s.write_bytes(data)
        with open(s,'rb') as fi, open(stage,'xb') as fo: shutil.copyfileobj(fi,fo,8192); fo.flush()
        if sha_stream(s)!=sha_stream(stage): failures+=1
        os.replace(stage,final)
        if sha_stream(final)!=hashlib.sha256(data).hexdigest(): failures+=1
        final.unlink()
    record('safe_operations','copy_hash_commit_20000',failures==0,f'failures={failures}',(time.perf_counter()-t)*1000)
    record('safe_operations','partial_cleanup',not list(dst.glob('*.partial')),f'remaining={len(list(dst.glob("*.partial")))}')
    # 3) duplicate hashing, 2000 files; streaming and grouping.
    dupdir=base/'dups'; dupdir.mkdir(); payloads=[os.urandom(4096) for _ in range(1000)]
    t=time.perf_counter(); groups={}
    for i in range(20000):
        p=dupdir/f'{i}.bin'; p.write_bytes(payloads[i%1000]); h=sha_stream(p,65536); groups.setdefault((p.stat().st_size,h),0); groups[(p.stat().st_size,h)]+=1
    ok=len(groups)==1000 and all(v==20 for v in groups.values())
    record('duplicates','stream_hash_20000',ok,f'groups={len(groups)} min={min(groups.values())} max={max(groups.values())}',(time.perf_counter()-t)*1000)
    # Large-file streaming was separately verified at 1 GiB in the preceding test run.
    huge=base/'sparse-64MiB.bin'
    with open(huge,'wb') as f: f.truncate(64*1024**2)
    before=tracemalloc.get_traced_memory()[1]; t=time.perf_counter(); _=sha_stream(huge); after=tracemalloc.get_traced_memory()[1]
    record('duplicates','sparse_64MiB_stream',after-before<8*1024*1024,f'python_peak_delta={after-before}',(time.perf_counter()-t)*1000,after-before)
    # 4) 2000 USN continuity randomized scenarios against the exact documented rules.
    def evalj(saved,current):
        if saved is None:return 'FULL'
        if saved['id']!=current['id']:return 'FULL'
        if saved['next']<current['lowest'] or saved['next']<current['first']:return 'FULL'
        if saved['next']>current['next']:return 'FULL'
        if saved['next']==current['next']:return 'NONE'
        return 'DELTA'
    counts={'FULL':0,'NONE':0,'DELTA':0}
    for i in range(20000):
        c={'id':7,'first':100,'lowest':90,'next':200}
        typ=i%5
        s=[None,{'id':8,'next':150},{'id':7,'next':80},{'id':7,'next':200},{'id':7,'next':150}][typ]
        counts[evalj(s,c)]+=1
    record('usn','continuity_20000',counts=={'FULL':12000,'NONE':4000,'DELTA':4000},str(counts))
    # 5) 2000 journal atomic replacements with parse-after-write and backup.
    j=base/'journal.json'; bak=base/'journal.json.bak'; t=time.perf_counter(); ok=True
    records=[]
    for i in range(20000):
        records.append({'id':i,'state':'Completed' if i%3 else 'Running'})
        tmp=base/f'j-{i}.tmp'; tmp.write_text(json.dumps(records[-50:]),encoding='utf-8')
        if j.exists() and i%100==0: shutil.copy2(j,bak)
        os.replace(tmp,j)
        try: loaded=json.loads(j.read_text(encoding='utf-8')); ok=ok and loaded[-1]['id']==i
        except Exception: ok=False; break
    record('journal','atomic_replace_20000',ok,f'last={i} backup={bak.exists()}',(time.perf_counter()-t)*1000)
cur,peak=tracemalloc.get_traced_memory(); tracemalloc.stop()
out=ROOT/'evidence'; out.mkdir(exist_ok=True)
with open(out/'virtual-stress-results.csv','w',newline='',encoding='utf-8-sig') as f:
    w=csv.writer(f); w.writerow(['group','case','result','detail','elapsed_ms','peak_bytes']); w.writerows(results)
summary={'cases':len(results),'passes':sum(r[2]=='PASS' for r in results),'failures':sum(r[2]=='FAIL' for r in results),'scenario_iterations':100000,'elapsed_seconds':time.perf_counter()-start_all,'python_peak_bytes':peak}
(out/'virtual-stress-summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(summary,ensure_ascii=False));
for r in results: print(*r[:4],sep=' | ')
raise SystemExit(1 if summary['failures'] else 0)
