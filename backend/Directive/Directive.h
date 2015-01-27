/*
 * Directive.h
 *
 *  Created on: 25 Mar 2014
 *      Author: np183
 */

#ifndef DIRECTIVE_H_
#define DIRECTIVE_H_

class Directive;

#include <string>
#include <vector>
#include "../Cell.h"
#include "../State.h"
#include "../CellProgram.h"
#include "../Event/Event.h"
#include "../Happening.h"


class Directive {
public:
	Directive()=delete;
	Directive(CellProgram* c);
	virtual ~Directive();

	virtual std::vector<std::string> programs() const=0;

//	// Return a vector of next events
//	// All corresponding to the same Cell!!!!!!
//	virtual std::vector<Event*> nextEvents(float,Cell*) const=0;

	virtual std::pair<Event*,std::vector<Happening*>> apply(Cell*,float duration, float time) const=0;

protected:
	CellProgram* _cProg;
};

#endif /* DIRECTIVE_H_ */
