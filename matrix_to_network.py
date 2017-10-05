from perseuspy import pd, nx
_, paramfile, infile, outfolder = sys.argv
G = nx.random_graphs.barabasi_albert_graph(10, 3)
network_table, networks = nx.to_perseus([G])
write_networks(outfolder, networks_table, networks)