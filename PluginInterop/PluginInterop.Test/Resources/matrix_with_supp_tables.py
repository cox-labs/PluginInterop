import sys
from perseuspy import pd
_, paramfile, infile, outfile, suppfile1, suppfile2 = sys.argv
df = pd.read_perseus(infile)
df.to_perseus(outfile)
df.to_perseus(suppfile1)
df.to_perseus(suppfile2)