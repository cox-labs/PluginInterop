"""
Create random barabasi albert graph

Requires passing n, m as additional arguments
"""
from perseuspy import pd, nx, write_networks
import sys
_, n, m, outfolder = sys.argv
G = nx.random_graphs.barabasi_albert_graph(int(n), int(m))
networks_table, networks = nx.to_perseus([G])
write_networks(outfolder, networks_table, networks)
