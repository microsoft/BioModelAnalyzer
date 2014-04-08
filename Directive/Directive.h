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
#include <random>
#include "../Cell.h"
#include "../State.h"
#include "../CellProgram.h"
#include "../Event/Event.h"


class Directive {
public:
	Directive()=delete;
	Directive(float m,float s,CellProgram* c);
	virtual ~Directive();

	virtual std::vector<std::string> programs() const=0;
	// Return a vector of next events
	// All corresponding to the same Cell!!!!!!
	virtual std::vector<Event*> nextEvents(float,Cell*, State*) const=0;
private:
	float _mean;
	float _sd;

protected:
	CellProgram* _cProg;
	static std::random_device _randomDev;
	static std::mt19937 _randomGen;

	float _randomTime(const float& mean, const float& sd) const;
	float _randomTime() const;
};

#endif /* DIRECTIVE_H_ */
