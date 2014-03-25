/*
 * Condition.h
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#ifndef CONDITION_H_
#define CONDITION_H_

class Condition;

#include <iosfwd>
#include <map>
#include "State.h"


class Condition {
public:
	Condition()=delete;
	Condition(const std::string& initializer);
	virtual ~Condition();

	std::pair<bool,unsigned int> evaluate(const State& st) const;

	bool operator==(const Condition& other) const;
	bool operator<(const Condition& other) const;
	friend std::ostream& operator<< (std::ostream&, const Condition&);
private:
	std::map<std::string,bool> _conjunction;
};

#endif /* CONDITION_H_ */
