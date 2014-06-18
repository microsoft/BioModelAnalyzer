#ifndef NEQEXP_H_
#define NEQEXP_H_


#include "EqExp.h"

class NeqExp :
	public EqExp
{
public:
	NeqExp()=delete;
	NeqExp(const std::string&, const std::string&);
	~NeqExp();

	virtual std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const;
	virtual std::string toString() const;
};

#endif