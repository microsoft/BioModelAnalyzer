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
// #include <tuple>
#include "../State.h"
#include "Directive.h"


class Divide: public Directive {
public:
	Divide()=delete;
	Divide(CellProgram* c,
		   std::string d1, State* s1, float m1, float sd1,
		   std::string d2, State* s2, float m2, float sd2);
	virtual ~Divide();

	virtual std::vector<std::string> programs() const;

//	std::tuple<const std::string&,const State*,float,float> daughter1() const;
//	std::tuple<const std::string& const State*,float,float> daughter2() const;


//	// Return a vector of next events
//	// All corresponding to the same Cell!!!!!!
//	virtual std::vector<Event*> nextEvents(float, Cell*) const;
	virtual std::pair<Event*,std::vector<Happening*>> apply(Cell*,float duration, float time) const;

private:
	std::string _daughter1;
	State* _st1;
	float _mean1;
	float _sd1;
	std::string _daughter2;
	State* _st2;
	float _mean2;
	float _sd2;
};

#endif /* DIVIDE_H_ */
