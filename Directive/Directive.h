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
#include "../Event/Event.h"


class Directive {
public:
	Directive()=delete;
	Directive(float m,float s);
	virtual ~Directive();

	virtual std::vector<std::string> programs() const=0;
	virtual std::vector<Event*> nextEvents(Event*) const=0;
private:
	float _mean;
	float _sd;

protected:
	static std::random_device _randomDev;
	static std::mt19937 _randomGen;

	float _randomTime(const float& mean, const float& sd) const;
};

#endif /* DIRECTIVE_H_ */
