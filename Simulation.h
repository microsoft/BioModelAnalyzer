/*
 * Simulation.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

class Simulation;

#ifndef SIMULATION_H_
#define SIMULATION_H_

#include <vector>
#include <set>
#include <iosfwd>
#include <string>
#include <utility>
#include <tuple>
#include "CellProgram.h"
#include "Event/Event.h"

class Simulation {
public:
	Simulation();
	Simulation(const std::string& filename);
	virtual ~Simulation();

	bool removeCell(CellProgram* c);
	bool addCell(CellProgram* c);

	void readFile(const std::string& filename);
	void run(const std::string& initial);
	void clear();

	std::pair<float,bool> overlap(const std::string&, const std::string&) const;

	CellProgram* program(const std::string&);
	unsigned int numPrograms() const;

	friend std::istream& operator>>(std::istream&, Simulation&);
	friend std::ostream& operator<<(std::ostream&, const Simulation&);
private:
	typedef std::tuple<std::string, std::string, float, float, std::string, std::string, std::string> _LineStructure;
	enum CsvFields { NAME, CONDITION, MEANTIME, STANDARDDEV, ACTION, DAUGHTER1, DAUGHTER2, LASTDELIM};

	_LineStructure _parseLine(const std::string& line) const ;

	float _currentTime;
	std::vector<Event*> _log;
	// std::set<Cell*> _activeCells;
	// std::set<Cell*> _allCells;
	std::map<std::string,CellProgram*> _programs;
};

#endif /* SIMULATION_H_ */
