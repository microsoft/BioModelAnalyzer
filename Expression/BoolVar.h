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
	~BoolVar();

	virtual std::pair<bool, unsigned int> evaluate(const State* st, const Simulation* sim, float from, float to) const;
	virtual std::string toString() const;
private:
	std::string _var;
};

#endif