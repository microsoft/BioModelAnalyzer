/*
 * Cell.h
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#ifndef CELL_H_
#define CELL_H_

#include <string>
#include <tuple>
#include "State.h"
#include "Condition.h"

class Cell {
public:
	Cell()=delete;
	Cell(const std::string& condition);
	~Cell();

	std::pair<bool,unsigned int> evaluate(const Condition& condition) const;
	bool update(const std::string& var, bool value);
private:
	State _state;
};

#endif /* CELL_H_ */
