#ifndef NEGATION_H_
#define NEGATION_H_

#include "BoolExp.h"

class Negation :
	public BoolExp
{
public:
	Negation() = delete;
	Negation(BoolExp*);
	Negation(const BoolExp&) = delete;
	Negation(BoolExp&&) = delete;
	Negation(const Negation&) = delete;
	Negation(Negation&&) = delete;

	~Negation();

	BoolExp* copy() const override;
	std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const override;
	std::string toString() const override;
private:
	BoolExp* _sub;
};

#endif