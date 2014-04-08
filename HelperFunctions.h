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

std::vector<std::string> splitOn(char c,const std::string& line);
std::string removeSpace(const std::string& in);
std::map<std::string,bool> splitConjunction(const std::string& initializer);

#endif /* HELPERFUNCTIONS_H_ */
