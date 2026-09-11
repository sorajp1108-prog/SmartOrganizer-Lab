# Source-level deterministic scenario validation mirroring JournalContinuityEvaluator.
def evaluate(saved,current):
    if saved is None:return 'FULL'
    if saved['id']!=current['id']:return 'FULL'
    if saved['next']<current['lowest'] or saved['next']<current['first']:return 'FULL'
    if saved['next']>current['next']:return 'FULL'
    if saved['next']==current['next']:return 'NONE'
    return 'DELTA'
c={'id':7,'first':100,'next':200,'lowest':90}
assert evaluate(None,c)=='FULL'
assert evaluate({'id':8,'next':150},c)=='FULL'
assert evaluate({'id':7,'next':80},c)=='FULL'
assert evaluate({'id':7,'next':200},c)=='NONE'
assert evaluate({'id':7,'next':150},c)=='DELTA'
print('PASS 5 journal continuity scenarios')
