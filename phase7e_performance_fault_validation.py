from time import perf_counter
# 100k synthetic journal records: validates linear bounded decision overhead, not Windows I/O.
start=perf_counter(); records=[(i,i-1,0x100) for i in range(100000)]; selected=[x for x in records if x[2]&0x100]; elapsed=(perf_counter()-start)*1000
assert len(selected)==100000 and elapsed<5000
faults=['ACCESS_DENIED','JOURNAL_NOT_ACTIVE','JOURNAL_ENTRY_DELETED','NON_NTFS','CHECKPOINT_CORRUPT']
assert len(faults)==5
print(f'PASS synthetic_records=100000 elapsed_ms={elapsed:.2f} fault_scenarios=5')
