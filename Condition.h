/*
 * Condition.h
 *
 *  Created on: 18 Mar 2014
 *      Author: np183
 */

#ifndef CONDITION_H_
#define CONDITION_H_

class Condition {
public:
	Condition();
	virtual ~Condition();

	virtual bool evaluate()=0;
};

#endif /* CONDITION_H_ */
