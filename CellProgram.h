/*
 * CellProgram.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */


class CellProgram;

#ifndef CELLPROGRAM_H_
#define CELLPROGRAM_H_

#include <string>
#include <map>
#include <iosfwd>
#include <memory>
#include <functional>
#include "Simulation.h"
#include "Condition.h"
#include "Directive/Directive.h"
// #include "Event/Event.h"
#include "Happening.h"

typedef std::map<Condition*,Directive*,std::function<bool(Condition* a,Condition* b)>> MyMapType;

class CellProgram {
public:
	CellProgram() = delete;
	CellProgram(const std::string& n, Simulation* s);
	virtual ~CellProgram();

	std::string name() const;
	Simulation* simulation() const;
	std::vector<Happening*> firstEvent(float currentTime, const std::string& state, float mean, float sd) const;

	//std::vector<Event*> firstEvent(float currentTime, State* state) const;
	//std::vector<Event*> nextEvent(float currentTime, Cell* cell) const;

	// const Directive* bestDirective(const State* state) const;
	const Directive* bestDirective(const State* state, float from, float to) const;
	std::vector<std::string> otherPrograms() const;

	void setDefaults(State*, float, float);
	void addCondition(Condition* c, Directive* d);

	const State* defState() const;
	float defMean() const;
	float defSD() const;

	friend std::ostream& operator<<(std::ostream&, const CellProgram&);

	class iterator {
	public:
		friend class CellProgram;

		iterator();
		iterator(const iterator&);
		iterator(iterator&&);
		~iterator();

		bool operator==(const iterator&) const;
		bool operator!=(const iterator&) const;
		iterator operator++();
		iterator operator++(int);
		Condition* operator->() const;
		Condition operator*() const;
	private:
		MyMapType::iterator _it;
	};

	CellProgram::iterator begin();
	CellProgram::iterator end();

private:
	std::string _name;
	Simulation* _sim;
	MyMapType _program;
	State* _defState;
	float _defMean;
	float _defSD;
	bool _conditionExists(Condition*) const;
};

#endif /* CELL_H_ */
