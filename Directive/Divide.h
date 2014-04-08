/*
 * Divide.h
 *
 *  Created on: 25 Mar 2014
 *      Author: np183
 */

#ifndef DIVIDE_H_
#define DIVIDE_H_

#include <string>
#include <vector>
#include "../State.h"
#include "Directive.h"


class Divide: public Directive {
public:
	Divide()=delete;
	Divide(float mean, float sd, CellProgram* c, std::string d1, State* s1, std::string d2, State* s2);
	virtual ~Divide();

	virtual std::vector<std::string> programs() const;

	// Return a vector of next events
	// All corresponding to the same Cell!!!!!!
	virtual std::vector<Event*> nextEvents(float, Cell*, State*) const;
private:
	std::string _daughter1;
	State* _st1;
	std::string _daughter2;
	State* _st2;
};

#endif /* DIVIDE_H_ */
