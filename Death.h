/*
 * Death.h
 *
 *  Created on: 14 Feb 2014
 *      Author: np183
 */

#ifndef DEATH_H_
#define DEATH_H_

#include <string>
#include "Event.h"

class Death: public Event {
public:
	Death()=delete;
	Death(const std::string&, Simulation* s=nullptr);
	virtual ~Death();

	virtual std::vector<Event*> execute() const;
	virtual void output(std::ostream&) const;

	virtual bool concerns(const std::string&) const;
	friend std::ostream& operator<<(std::ostream&,const Death&);
private:
	std::string _cell;
};

#endif /* DEATH_H_ */
