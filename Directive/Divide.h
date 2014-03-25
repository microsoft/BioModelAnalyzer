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
#include "Directive.h"


class Divide: public Directive {
public:
	Divide()=delete;
	Divide(float mean, float sd, std::string d1, std::string d2);
	virtual ~Divide();

	virtual std::vector<std::string> programs() const;
	virtual std::vector<Event*> nextEvents(Event*) const;
private:
	std::string _daughter1;
	std::string _daughter2;
};

#endif /* DIVIDE_H_ */
