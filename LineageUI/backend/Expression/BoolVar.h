// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#ifndef BOOLVAR_H_ 
#define BOOLVAR_H_

#include "BoolExp.h"
#include "../Variable/Variable.h"

class BoolVar :
	public BoolExp
{
public:
	BoolVar()=delete;
	BoolVar(const std::string&);
	BoolVar(const BoolExp&) = delete;
	BoolVar(BoolExp&&) = delete;
	BoolVar(const BoolVar&) = delete;
	BoolVar(BoolVar&&) = delete;
	~BoolVar();

	BoolExp* copy() const override;
	std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const override;
	std::string toString() const override;

private:
	std::string _var;
};

#endif
