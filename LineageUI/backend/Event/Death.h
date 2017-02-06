// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
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
	Death(const std::string&, /*Simulation* s=nullptr,*/ Cell* c=nullptr);
	virtual ~Death();

//	virtual std::vector<Event*> execute() const;
	void output(std::ostream&) const override;

	bool concerns(const std::string&) const override;
	bool expressed(const std::string& cell,const std::string& var) const override;

	std::string toString() const override;
	friend std::ostream& operator<<(std::ostream&,const Death&);
private:
	std::string _cell;
};

#endif /* DEATH_H_ */
