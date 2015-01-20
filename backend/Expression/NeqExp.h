#ifndef NEQEXP_H_
#define NEQEXP_H_


#include "BoolExp.h"
#include "EqExp.h"

class NeqExp : public EqExp
{
public:
	NeqExp()=delete;
	NeqExp(const std::string&, const std::string&);
	NeqExp(const BoolExp&) = delete;
	NeqExp(BoolExp&&) = delete;
	NeqExp(const NeqExp&) = delete;
	NeqExp(NeqExp&&) = delete;
	~NeqExp();

	BoolExp* copy() const override;
	std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const override;
	std::string toString() const override;
};

#endif