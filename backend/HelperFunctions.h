/*
 * HelperFunctions.h
 *
 *  Created on: 19 Mar 2014
 *      Author: np183
 */

#ifndef HELPERFUNCTIONS_H_
#define HELPERFUNCTIONS_H_

#include <string>
#include <vector>
#include <map>

#include "Variable/Variable.h"
#include "Expression/BoolExp.h"

std::vector<std::string> splitOn(char c,const std::string& line);
std::string removeSpace(const std::string& in);
BoolExp* parseSimpleBoolExp(const  std::string& exp);
BoolExp* parseBoolExp(const std::string& boolexp);
std::map<std::string,Variable*> splitConjunction(const std::string& initializer,const Simulation*);

#endif /* HELPERFUNCTIONS_H_ */
