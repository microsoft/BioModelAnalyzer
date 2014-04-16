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
	bool expressed(const std::string&) const;

	bool update(const std::string& var, bool value);

	const std::string name() const;

	bool alive() const;
	void kill();
private:
	bool _alive;
	State* _state;
	const CellProgram* _program;
};

#endif /* CELL_H_ */
