/*
 * Division.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#ifndef DIVISION_H_
#define DIVISION_H_

#include <string>
#include <iosfwd>
#include "Event.h"

class Division: public Event {
public:
	Division() =delete;
	Division(const std::string& p, const std::string& d1, const std::string& d2, float d, float t, Simulation* s);
	virtual ~Division();

	std::string parent() const;
	std::string daughter1() const;
	std::string dauthger2() const;

	void setParent(const std::string& p);
	void setDaughter1(const std::string& d1);
	void setDaughter2(const std::string& d2);

	virtual std::vector<Event*> execute() const;
	virtual void output(std::ostream&) const;
	virtual bool concerns(const std::string&) const;


	friend std::ostream& operator<<(std::ostream&,const Division&);
private:
	std::string _parent;
	std::string _daughter1;
	std::string _daughter2;
};

#endif /* DIVISION_H_ */
