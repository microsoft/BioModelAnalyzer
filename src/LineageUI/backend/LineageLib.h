#ifndef LINEAGELIB_H_
#define LINEAGELIB_H_

#include <vector>
#include <string>

std::vector<std::string> programs(std::vector<std::string> pgm);
std::vector<std::string> conditions(std::vector<std::string> pgm, std::string program);
std::vector<std::string> simulate(std::vector<std::string> pgm, std::string condition);
std::vector<std::string> checkTimeOverlap(std::vector<std::string> pgm, std::string condition, std::string firstcell, std::string secondcell, unsigned int num_of_simulutions,bool rawData=false);
std::vector<std::string> cellExistence(std::vector<std::string> pgm, std::string condition, unsigned int num_of_simultions);
std::vector<std::string> simulateAbnormal(std::vector<std::string> pgm, std::string condition, unsigned int upper_bound);

#endif /* LINEAGELIB_H_ */