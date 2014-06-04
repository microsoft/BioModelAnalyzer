#ifndef VARIABLE_H_
#define VARIABLE_H_

#include <string>
#include <iosfwd>
#include "Type.h"
#include "EnumType.h"

class Variable
{
public:
	Variable()=delete;
	Variable(const Variable&) = delete;
	Variable& operator=(const Variable&) = delete;

	Variable(const std::string&, bool val); // Create a Bool Variable
	Variable(const std::string&, EnumType* t, EnumType::Value*); // Create an Enumerated 
	~Variable();

	bool operator==(const Variable& other) const;
	bool operator!=(const Variable& other) const;

	friend std::ostream& operator<<(std::ostream&, const Variable&);
private:
	std::string _name;
	Type* _type; // Point to the type owned by someone else
	Type::Value* _val; // Own this
};


#endif