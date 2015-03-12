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
#include "Variable/Type.h"
#include "Variable/EnumType.h"

class Simulation {
public:
	Simulation();
	Simulation(const Simulation&) = delete;
	Simulation(Simulation&&) = delete;
	Simulation(const std::string& filename);
	Simulation(const std::vector<std::string>&);
	virtual ~Simulation();

//	bool addCellProgram(CellProgram* c);
	void addCell(Cell* c);
	std::vector<Cell*> cells(const std::string& name) const;

	void readFile(const std::string& filename);
	void readVector(const std::vector<std::string>& programs);
	void run(const std::string& initialProg,
			 const std::string& initialState,
			 float initialMean, float initialSD);
	void clear();

	std::pair<float,bool> overlap(const std::string&, const std::string&) const;
	std::map<std::string,unsigned int> cellCount() const;

	CellProgram* program(const std::string&);
	unsigned int numPrograms() const;
	std::vector<std::string> programs() const;

	const Type* type(const std::string&) const;
	const EnumType* cellCycleType() const;

	bool expressed(const std::string&,float from, float to) const;

	std::pair<float, float> defTime(const std::string&) const;

	std::string toString(unsigned int num=0) const;
	std::vector<std::string> toVectorString() const;
	std::string toJson() const;
	friend std::istream& operator>>(std::istream&, Simulation&);
	friend std::ostream& operator<<(std::ostream&, const Simulation&);

	static const std::string G1_PHASE; // { "G1" };
	static const std::string G2_PHASE; // { "G2" };
	static const std::string S_PHASE; // {  "S" };
	static const std::string G0_PHASE; // { "G0" };
private:
	class EventPtrComparison
	{
	public:
	  bool operator() (Happening* lhs,  Happening* rhs) const;
	};


//	_LineStructure _parseLine(const std::string& line) const ;
	void _parseLine(const std::string& line);
	void _sanitize(std::string& buffer);
	bool _validCellCycle(const std::string&) const;
	void _addTypesFromConjunction(const std::string&);
	void _addCellCycleType();
	void _setDefaultTime(const std::string&, float, float);
	std::string _setDefaultNextAction(const std::string &) const;
	// std::pair<std::string,State*> _parseCellWithState(const std::string& cell) const;

	float _currentTime;
	std::vector<Event*> _log;
	// TODO: Change to multiset and set that are sorted by the name
	//       of the object in them.
	std::map<std::string, std::pair<float, float>> _defaults;
	std::map<std::string, Type*> _types;
	std::multimap<std::string,Cell*> _cells;
	std::map<std::string,CellProgram*> _programs;
};

#endif /* SIMULATION_H_ */
