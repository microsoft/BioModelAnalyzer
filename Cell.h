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

class Cell;

#include "CellProgram.h"
#include "State.h"
#include "Condition.h"

class Cell {
public:
	Cell()=delete;
	Cell(const CellProgram*,const std::string& state);
	Cell(const CellProgram*,State*);
	~Cell();

	std::pair<bool,unsigned int> evaluate(Condition* condition) const;
	bool update(const std::string& var, bool value);
private:
	State* _state;
	const CellProgram* _program;
};

#endif /* CELL_H_ */
