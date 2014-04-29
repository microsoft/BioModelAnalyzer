/*
 * Birth.h
 *
 *  Created on: 25 Apr 2014
 *      Author: np183
 */

#ifndef BIRTH_H_
#define BIRTH_H_

#include <string>
#include <iosfwd>
#include "Event.h"

class Birth: public Event {
public:
	Birth() =delete;
	Birth(const std::string me, State* st, float d, float t, Simulation* s, Cell* c);

	virtual ~Birth();

	std::string cellName() const;

	void setCell(const std::string& c);

	virtual std::vector<Event*> execute() const;
	virtual void output(std::ostream&) const;
	virtual bool concerns(const std::string&) const;

	virtual std::string toString() const;

	friend std::ostream& operator<<(std::ostream&, const Birth&);
private:
	std::string _cellName;
	State* _st;
};

#endif /* BIRTH_H_ */
