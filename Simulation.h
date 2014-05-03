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
#include <map>
#include <iosfwd>
#include <string>
#include <utility>
#include <tuple>

class Simulation;

#include "CellProgram.h"
#include "Condition.h"
#include "Event/Event.h"
#include "Directive/Directive.h"
#include "Happening.h"

class Simulation {
public:
	Simulation();
	Simulation(const std::string& filename);
	virtual ~Simulation();

//	bool addCellProgram(CellProgram* c);
	void addCell(Cell* c);
	std::vector<Cell*> cells(const std::string& name) const;

	void readFile(const std::string& filename);
	void run(const std::string& initialProg,
			 const std::string& initialState,
			 float initialMean, float initialSD);
	void clear();

	std::pair<float,bool> overlap(const std::string&, const std::string&) const;
	std::map<std::string,unsigned int> cellCount() const;

	CellProgram* program(const std::string&);
	unsigned int numPrograms() const;

	bool expressed(const std::string&,float from, float to) const;

	std::string toString(unsigned int num=0) const;
	friend std::istream& operator>>(std::istream&, Simulation&);
	friend std::ostream& operator<<(std::ostream&, const Simulation&);
private:
	enum CsvFields { NAME, CONDITION, ACTION,
					 DAUGHTER1, STATE1, MEANTIME1, STANDARDDEV1,
					 DAUGHTER2, STATE2, MEANTIME2, STANDARDDEV2,
					 LASTDELIM};

	class EventPtrComparison
	{
	public:
	  bool operator() (Happening* lhs,  Happening* rhs) const;
	};


//	_LineStructure _parseLine(const std::string& line) const ;
	void _parseLine(const std::string& line);
	float _readFloat(const std::string& input) const;
	void _sanitize(std::string& buffer);
	// std::pair<std::string,State*> _parseCellWithState(const std::string& cell) const;

	float _currentTime;
	std::vector<Event*> _log;
	std::multimap<std::string,Cell*> _cells;
	std::map<std::string,CellProgram*> _programs;
};

#endif /* SIMULATION_H_ */
