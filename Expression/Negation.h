#ifndef NEGATION_H_
#define NEGATION_H_

#include "BoolExp.h"

class Negation :
	public BoolExp
{
public:
	Negation(BoolExp*);
	~Negation();

	virtual std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const;
	virtual std::string toString() const;
private:
	BoolExp* _sub;
};

#endif