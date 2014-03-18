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
#include "CellProgram.h"
#include "Event.h"

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
	void _parseLine(const std::string& line,std::string& name,float& mean, float& sd, std::string& d1,std::string& d2);


	float _currentTime;
	std::vector<Event*> _log;
	// std::set<Cell*> _activeCells;
	// std::set<Cell*> _allCells;
	std::map<std::string,CellProgram*> _programs;
};

#endif /* SIMULATION_H_ */
