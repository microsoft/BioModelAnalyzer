/*
 * State.h
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#ifndef STATE_H_
#define STATE_H_

#include <iosfwd>
#include <map>
#include <string>

class State;

#include "Simulation.h"
#include "Condition.h"
#include "Variable/Type.h"
#include "Variable/Variable.h"

class State {
public:
	State()=delete;
	State(const std::string& initializer);
	State(const State&);
	~State();

	// std::pair<bool,unsigned int> evaluate(const Condition&) const;

	// Returns if the variable exists and its polarity
	std::pair<bool, const Type::Value*> value(const std::string& var) const;

	// If the variable exists update it and return true
	// If the variable does not exist add it with this value and return false
	bool set(const std::string& var, bool val);
	bool set(const std::string& var, const Type::Value&);

	// Set all the variables in the other state
	// Return false if at least one did not exist initially
	bool set(const State* other);

	// If the variable exists update it and return true
	// If the variable does not exist return false
	bool update(const std::string& var,bool val);
	bool update(const std::string& var, const Type::Value&);


	// Create a fresh copy of this state
	// Overwrite the values of variables according to the other state
	// Add the values of new variables
	State* copyOverwrite(const State*) const;

	std::string toString() const;
	friend std::ostream& operator<< (std::ostream&, const State&);
private:
	// TODO: instead of a map it would make more sense to use a set
	//       with custom comparison
	std::map<std::string, Variable*> _vars;
};

#endif /* STATE_H_ */
