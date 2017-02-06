// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#ifndef ANDEXP_H_
#define ANDEXP_H_

#include "BoolExp.h"

class AndExp :
	public BoolExp
{
public:
	AndExp()=delete;
	AndExp(const BoolExp&) = delete;
	AndExp(BoolExp&&) = delete;
	AndExp(const AndExp&) = delete;
	AndExp(AndExp&&) = delete;
	AndExp(BoolExp*, BoolExp*);
	~AndExp();

	BoolExp* copy() const override;
	std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const override;
	std::string toString() const override;

private:
	BoolExp* _sub1;
	BoolExp* _sub2;
};

#endif
