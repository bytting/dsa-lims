
identified_isotopes = {}

def read_isotopes(fp):
	line = fp.readline()

	while line:
		line = line.strip()

		if line.startswith("THE FOLLOWING PEAKS WERE NOT IDENTIFIED"):
			return		
		
		if line:
			items = line.split()
			if len(items) == 5:
				identified_isotopes[items[1]] = [float(items[3]), float(items[4]), -1.0]

		line = fp.readline()


def read_MDAs(fp):
	line = fp.readline()

	while line:
		line = line.strip()
		
		if line:
			items = line.split()
			if len(items) == 2 and items[0] in identified_isotopes:				
				identified_isotopes[items[0]][2] = float(items[1])

		line = fp.readline()


with open(filename) as fp:
	line = fp.readline()

	while line:
		line = line.strip()

		if line.startswith("SPECTRUM NO"):
			items = line.split()
			spectrum_reference = ''.join(items[3:])

		elif line.startswith("NUCLIDE LIBRARY"):
			items = line.split()
			nuclide_library = items[3]

		elif line.startswith("DETECTION LIMIT LIB"):
			items = line.split()
			detection_limit_lib = items[4]

		elif line.startswith("THE FOLLOWING ISOTOPES WERE IDENTIFIED"):
			read_isotopes(fp)

		elif line.startswith("Detection limits"):
			read_MDAs(fp)

		line = fp.readline()

#spectrum_reference = "G1 14 1627"

#identified_isotopes = {}
#identified_isotopes["Cs-137"] = [0.12, 6.1079E+02, 0.01]
#identified_isotopes["Co-60"] = [0.22, 1.2, 0.11]
#identified_isotopes["Co-62"] = [1.4, 3.2, 0.02]
#dentified_isotopes["Am-241"] = [0.7, 3.2, 0.01]

#detection_limits = {}
#detection_limits["Cs-137"] = 1.1E+03
#etection_limits["Co-60"] = 1.4E+00