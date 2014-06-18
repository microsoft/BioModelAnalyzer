#ifndef EQEXP_H_
#define EQEXP_H_

#include "BoolExp.h"

class EqExp :
	public BoolExp
{
public:
	EqExp()=delete;
	EqExp(const std::string&, const std::string&);
	~EqExp();

	virtual std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const;
	virtual std::string toString() const;

protected:
	std::string _var;
	std::string _val;
};

#endif

