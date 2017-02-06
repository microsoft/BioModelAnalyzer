// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#ifndef LINEAGELIB_H_
#define LINEAGELIB_H_

#include <vector>
#include <string>

std::vector<std::string> simulate(std::vector<std::string> pgm, std::string condition);
std::vector<std::string> checkTimeOverlap(std::vector<std::string> pgm, std::string condition, std::string firstcell, std::string secondcell, int num_of_simulutions,bool rawData=false);
std::vector<std::string> cellExistence(std::vector<std::string> pgm, std::string condition, int num_of_simultions);
std::vector<std::string> simulateAbnormal(std::vector<std::string> pgm, int upper_bound);

#endif /* LINEAGELIB_H_ */
