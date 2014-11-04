#ifndef EQEXP_H_
#define EQEXP_H_

#include "BoolExp.h"

class EqExp :
	public BoolExp
{
public:
	EqExp()=delete;
	EqExp(const BoolExp&) = delete;
	EqExp(BoolExp&&) = delete;
	EqExp(const EqExp&) = delete;
	EqExp(EqExp&&) = delete;
	EqExp(const std::string&, const std::string&);
	~EqExp();

	BoolExp* copy() const override;
	std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const override;
	std::string toString() const override;

protected:
	std::string _var;
	std::string _val;
};

#endif

