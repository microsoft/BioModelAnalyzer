#ifndef ANDEXP_H_
#define ANDEXP_H_

#include "BoolExp.h"

class AndExp :
	public BoolExp
{
public:
	AndExp()=delete;
	AndExp(BoolExp*, BoolExp*);
	~AndExp();

	virtual std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const;
	virtual std::string toString() const;

private:
	BoolExp* _sub1;
	BoolExp* _sub2;
};

#endif
