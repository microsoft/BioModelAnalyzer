// Copyright (c) Microsoft Research 2017
// License: MIT. See LICENSE

#include "stdafx.h"
#include "Attractors.h"

inline int logTwo(unsigned int i) {
    unsigned int r = 0;
    while (i >>= 1) r++;
    return r;
}

inline int bits(unsigned int i) {
    return i == 0 ? 0 : logTwo(i) + 1;
}

inline bool nthBitSet(int i, int n) {
    return (1 << n) & i;
}

inline BDD logicalEquivalence(const BDD& a, const BDD& b) {
    return !(a ^ b);
}

std::string printRange(const std::list<int>& values) {
    std::string s("[");
    s += std::to_string(values.front());
    std::for_each(std::next(values.begin()), values.end(), [&s](int b) { s += ";"; s += std::to_string(b); });
    s += "]";
    return s;
}

std::vector<int> parseRange(const std::string& range) {
    if (range.at(0) != '[') return std::vector<int>(1, std::stoi(range));

    std::string copy = range.substr(1, range.size() - 2);
    std::vector<int> result;
    std::istringstream iss(copy);
    std::string s;
    while (std::getline(iss, s, ';')) result.push_back(std::stoi(s));

    return result;
}

std::string fromBinary(const std::string& bits, int offset) {
    int i = 0;
    auto lambda = [&i](int a) { return a + std::pow(2.0f, i); };
    std::list<int> values{ offset };

    for (auto it = std::begin(bits); it < std::end(bits); ++it) {
        if (*it == '-') {
            std::list<int> copy(values);
            std::transform(copy.begin(), copy.end(), copy.begin(), lambda);
            values.splice(values.end(), copy);
        }
        else if (*it == '1') {
            std::transform(values.begin(), values.end(), values.begin(), lambda);
        }
        i++;
    }
    return values.size() > 1 ? printRange(values) : std::to_string(values.front());
}

BDD Attractors::representState(const std::vector<bool>& values) const {
    BDD bdd = manager.bddOne();
    for (int i = 0; i < values.size(); i++) {
        BDD var = manager.bddVar(i);
        if (!values[i]) {
            var = !var;
        }
        bdd *= var;
    }
    return bdd;
}

BDD Attractors::representNonPrimeVariables() const {
    return representState(std::vector<bool>(numUnprimedBDDVars, true));
}

BDD Attractors::representPrimeVariables() const {
    BDD bdd = manager.bddOne();
    for (int i = numUnprimedBDDVars; i < numUnprimedBDDVars * 2; i++) {
        BDD var = manager.bddVar(i);
        bdd *= var;
    }
    return bdd;
}

inline int Attractors::countBits(int end) const {
    auto lambda = [](int a, int b) { return a + bits(b); };
    return std::accumulate(ranges.begin(), ranges.begin() + end, 0, lambda);
}

BDD Attractors::representUnprimedVarQN(int var, int val) const {
    BDD bdd = manager.bddOne();
    int i = countBits(var);

    int b = bits(ranges[var]);
    for (int n = 0; n < b; n++) {
        BDD var = manager.bddVar(i);
        if (!nthBitSet(val, n)) {
            var = !var;
        }
        bdd *= var;
        i++;
    }

    return bdd;
}

BDD Attractors::representPrimedVarQN(int var, int val) const {
    BDD bdd = manager.bddOne();
    int i = numUnprimedBDDVars + countBits(var);

    int b = bits(ranges[var]);
    for (int n = 0; n < b; n++) {
        BDD var = manager.bddVar(i);
        if (!nthBitSet(val, n)) {
            var = !var;
        }
        bdd *= var;
        i++;
    }

    return bdd;
}

BDD Attractors::representStateQN(const std::vector<int>& vars, const std::vector<int>& values) const {
    BDD bdd = manager.bddOne();
    for (size_t i = 0; i < vars.size(); i++) {
        int var = vars[i];
        int val = values[i];
        bdd *= representUnprimedVarQN(var, val);
    }
    return bdd;
}

BDD Attractors::varDoesChangeQN(int var) const {
    int start = countBits(var);
    int numBits = bits(ranges[var]);

    BDD bdd = manager.bddZero();
    for (int i = start; i < start + numBits; i++) {
        BDD v = manager.bddVar(i);
        BDD vPrime = manager.bddVar(i + numUnprimedBDDVars);
        bdd += v ^ vPrime;
    }
    return bdd;
}

BDD Attractors::otherVarsDoNotChangeQN(int var) const {
    BDD bdd = manager.bddOne();
    for (int i = 0; i < ranges.size(); i++) {
        if (i != var) {
            bdd *= !varDoesChangeQN(i);
        }
    }

    return bdd;
}

BDD Attractors::representSyncQNTransitionRelation() const {
    BDD bdd = manager.bddOne();

    for (int v = 0; v < ranges.size(); v++) {
        if (ranges[v] > 0) {
            const auto& iVars = qn.inputVars[v];
            const auto& iValues = qn.inputValues[v];
            const auto& oValues = qn.outputValues[v];

            std::vector<BDD> states(ranges[v] + 1, manager.bddZero());
            for (int i = 0; i < oValues.size(); i++) {
                states[oValues[i]] += representStateQN(iVars, iValues[i]);
            }
            for (int val = 0; val <= ranges[v]; val++) {
                BDD vPrime = representPrimedVarQN(v, val);
                bdd *= logicalEquivalence(states[val], vPrime);
            }
        }
    }
    return bdd;
}

BDD Attractors::representAsyncQNTransitionRelation() const {
    BDD fixpoint = manager.bddOne();
    for (int i = 0; i < numUnprimedBDDVars; i++) {
        BDD v = manager.bddVar(i);
        BDD vPrime = manager.bddVar(numUnprimedBDDVars + i);
        fixpoint *= logicalEquivalence(v, vPrime);
    }

    BDD bdd = manager.bddZero();

    for (int v = 0; v < ranges.size(); v++) {
        if (ranges[v] > 0) {
            const auto& iVars = qn.inputVars[v];
            const auto& iValues = qn.inputValues[v];
            const auto& oValues = qn.outputValues[v];
            std::vector<BDD> states(ranges[v] + 1, manager.bddZero());

            for (int i = 0; i < oValues.size(); i++) {
                states[oValues[i]] += representStateQN(iVars, iValues[i]);
            }
            BDD updates = manager.bddOne();
            for (int val = 0; val <= ranges[v]; val++) {
                BDD vPrime = representPrimedVarQN(v, val);
                updates *= logicalEquivalence(states[val], vPrime);
            }
            BDD transition = updates * otherVarsDoNotChangeQN(v) * (fixpoint + varDoesChangeQN(v));
            bdd += transition;
        }
    }

    return bdd;
}

BDD Attractors::renameRemovingPrimes(const BDD& bdd) const {
    int *permute = new int[numUnprimedBDDVars * 2];
    for (int i = 0; i < numUnprimedBDDVars; i++) {
        permute[i] = i;
        permute[i + numUnprimedBDDVars] = i;
    }
    BDD r = bdd.Permute(permute);
    delete[] permute;
    return r;
}

BDD Attractors::renameAddingPrimes(const BDD& bdd) const {
    int *permute = new int[numUnprimedBDDVars * 2];
    for (int i = 0; i < numUnprimedBDDVars; i++) {
        permute[i] = i + numUnprimedBDDVars;
        permute[i + numUnprimedBDDVars] = i + numUnprimedBDDVars;
    }

    BDD r = bdd.Permute(permute);
    delete[] permute;
    return r;
}

BDD Attractors::randomState(const BDD& S) const {
    char *out = new char[numUnprimedBDDVars * 2];
    S.PickOneCube(out);
    std::vector<bool> values;
    for (int i = 0; i < numUnprimedBDDVars; i++) {
        if (out[i] == 0) {
            values.push_back(false);
        }
        else {
            values.push_back(true);
        }
    }
    delete[] out;
    return representState(values);
}

void Attractors::removeInvalidBitCombinations(BDD& S) const {
    for (int var = 0; var < ranges.size(); var++) {
        if (ranges[var] > 0) {
            int b = bits(ranges[var]);
            int theoreticalMax = (1 << b) - 1;

            for (int val = ranges[var] + 1; val <= theoreticalMax; val++) {
                S *= !representUnprimedVarQN(var, val);
            }
        }
    }
}

BDD Attractors::immediateSuccessorStates(const BDD& transitionBdd, const BDD& valuesBdd) const {
    BDD bdd = transitionBdd * valuesBdd;
    bdd = bdd.ExistAbstract(nonPrimeVariables);
    return renameRemovingPrimes(bdd);
}

BDD Attractors::forwardReachableStates(const BDD& transitionBdd, const BDD& valuesBdd) const {
    BDD reachable = manager.bddZero();
    BDD frontier = valuesBdd;

    while (!frontier.IsZero()) {
        frontier = immediateSuccessorStates(transitionBdd, frontier) * !reachable;
        reachable += frontier;
    }
    return reachable;
}

BDD Attractors::immediatePredecessorStates(const BDD& transitionBdd, const BDD& valuesBdd) const {
    BDD bdd = renameAddingPrimes(valuesBdd);
    bdd *= transitionBdd;
    return bdd.ExistAbstract(primeVariables);
}

BDD Attractors::backwardReachableStates(const BDD& transitionBdd, const BDD& valuesBdd) const {
    BDD reachable = manager.bddZero();
    BDD frontier = valuesBdd;

    while (!frontier.IsZero()) {
        frontier = immediatePredecessorStates(transitionBdd, frontier) * !reachable;
        reachable += frontier;
    }
    return reachable;
}

BDD Attractors::fixpoints(const BDD& syncTransitionBdd) const {
    BDD fixpoint = manager.bddOne();
    for (int i = 0; i < numUnprimedBDDVars; i++) {
        BDD v = manager.bddVar(i);
        BDD vPrime = manager.bddVar(numUnprimedBDDVars + i);
        fixpoint *= logicalEquivalence(v, vPrime);
    }

    BDD bdd = renameRemovingPrimes(syncTransitionBdd * fixpoint);
    removeInvalidBitCombinations(bdd);
    return bdd;
}

std::list<BDD> Attractors::attractors(const BDD& transitionBdd, const BDD& statesToRemove) const {
    std::list<BDD> attractors;
    BDD S = manager.bddOne();
    removeInvalidBitCombinations(S);
    S *= !statesToRemove;

    while (!S.IsZero()) {
        BDD s = randomState(S);

        for (int i = 0; i < ranges.size(); i++) { // unrolling by ranges.size() may not be the perfect choice of number
            BDD sP = immediateSuccessorStates(transitionBdd, s);
            s = randomState(sP);
        }

        BDD fr = forwardReachableStates(transitionBdd, s);
        BDD br = backwardReachableStates(transitionBdd, s);

        if ((fr * !br).IsZero()) {
            attractors.push_back(fr);
        }

        S *= !(s + br);
    }
    return attractors;
}

bool Attractors::isAsyncLoop(const BDD &S, const BDD& syncTransitionBdd) const {
    BDD reached = manager.bddZero();
    BDD s = randomState(S);

    while (!s.IsZero()) {
        BDD sP = immediateSuccessorStates(syncTransitionBdd, s); // sync, so should be one state
        char *sCube = new char[numUnprimedBDDVars * 2];
        s.PickOneCube(sCube);
        char *sPCube = new char[numUnprimedBDDVars * 2];
        sP.PickOneCube(sPCube);

        int nVarDiff = 0;
        int i = 0;
        for (int range : ranges) {
            int b = bits(range);
            for (int j = i; j < i + b; j++) {
                if (sCube[j] != sPCube[j]) {
                    nVarDiff++;
                    break;
                }
            }
            if (nVarDiff >= 2) return false;
            i += b;
        }
        s = sP * !reached;
        reached += s;
    }
    return true;
}

std::string Attractors::prettyPrint(const BDD& attractor) const {
    // ideally would not use a temp file
    FILE *old = manager.ReadStdout();
    FILE *fp = fopen("temp.txt", "w");
    manager.SetStdout(fp);
    attractor.PrintMinterm();
    manager.SetStdout(old);
    fclose(fp);

    std::string out;
    std::ifstream infile("temp.txt");
    std::string line;
    auto lambda = [](const std::string& a, const std::string& b) { return a + "," + b; };
    while (std::getline(infile, line)) {
        std::list<std::string> output;
        int i = 0;
        for (int v = 0; v < ranges.size(); v++) {
            if (ranges[v] == 0) {
                output.push_back(std::to_string(minValues[v]));
            }
            else {
                int b = bits(ranges[v]);
                output.push_back(fromBinary(line.substr(i, b), minValues[v]));
                i += b;
            }
        }

        out += std::accumulate(std::next(output.begin()), output.end(), output.front(), lambda) + "\n";
    }

    return out;
}

BDD Attractors::readStatesFromCsv(const std::string& filename) const {
    if (filename.empty()) return manager.bddOne();

    BDD initial = manager.bddZero();
    std::ifstream infile(filename);
    std::string line;
    std::getline(infile, line); // skip header
    while (std::getline(infile, line)) {
        line.erase(std::remove_if(line.begin(), line.end(), isspace), line.end()); // remove all whitespace
        BDD state = manager.bddOne();
        std::istringstream iss(line);
        std::string s;
        int var = 0;
        while (std::getline(iss, s, ',')) {
            std::vector<int> vals(parseRange(s));
            BDD bdd = manager.bddZero();
            for (int val : vals) {
                bdd += representUnprimedVarQN(var, val - minValues[var]);
            }
            state *= bdd;
            var++;
        }
        initial += state;
    }
    return initial;
}

int Attractors::runSync(const BDD& initialStates, const std::string& outputFile, const std::string& header) const {
    std::cout << "Building synchronous transition relation..." << std::endl;
    BDD syncTransitionBdd = representSyncQNTransitionRelation();
    if (syncTransitionBdd.IsZero()) {
        std::cout << "TransitionBDD is zero!" << std::endl;
        return 1;
    }

    BDD statesToRemove = !initialStates;
    if (initialStates.IsOne()) { // fixpoint optimisation only works if we are starting from all possible initial states
        std::cout << "Finding fixpoints..." << std::endl;
        BDD fix = fixpoints(syncTransitionBdd);

        if (!fix.IsZero()) {
            std::ofstream file(outputFile + "Fixpoints.csv");
            file << header << std::endl;
            file << prettyPrint(fix) << std::endl;
        }

        statesToRemove = fix + backwardReachableStates(syncTransitionBdd, fix);
    }

    std::cout << "Finding attractors..." << std::endl;
    std::list<BDD> syncLoops = attractors(syncTransitionBdd, statesToRemove);

    int i = 0;
    for (const BDD& attractor : syncLoops) {
        std::ofstream file(outputFile + "Attractor" + std::to_string(i) + ".csv");
        file << header << std::endl;
        file << prettyPrint(attractor) << std::endl;
        i++;
    }

    return 0;
}

int Attractors::runAsync(const BDD& initialStates, const std::string& outputFile, const std::string& header) const {
    std::cout << "Building synchronous transition relation..." << std::endl;
    BDD syncTransitionBdd = representSyncQNTransitionRelation();
    if (syncTransitionBdd.IsZero()) {
        std::cout << "TransitionBDD is zero!" << std::endl;
        return 1;
    }

    BDD statesToRemove = !initialStates;
    BDD fix = manager.bddZero();
    if (initialStates.IsOne()) { // fixpoint optimisation only works if we are starting from all possible initial states
        std::cout << "Finding fixpoints..." << std::endl;
        fix = fixpoints(syncTransitionBdd);

        if (!fix.IsZero()) {
            std::ofstream file(outputFile + "Fixpoints.csv");
            file << header << std::endl;
            file << prettyPrint(fix) << std::endl;
        }

        statesToRemove = fix + backwardReachableStates(syncTransitionBdd, fix);
    }

    std::cout << "Finding attractors..." << std::endl;
    std::list<BDD> syncLoops = attractors(syncTransitionBdd, statesToRemove);

    std::cout << "Building asynchronous transition relation..." << std::endl;
    BDD asyncTransitionBdd = representAsyncQNTransitionRelation();
    std::cout << "Finding loop attractors..." << std::endl;
    std::list<BDD> asyncLoops;

    BDD syncAsyncAttractors = fix;
    for (const BDD& l : syncLoops) {
        if (isAsyncLoop(l, syncTransitionBdd)) {
            syncAsyncAttractors += l;
            asyncLoops.push_back(l);
        }
    }

    BDD br = syncAsyncAttractors + backwardReachableStates(asyncTransitionBdd, syncAsyncAttractors);
    asyncLoops.splice(asyncLoops.end(), attractors(asyncTransitionBdd, br));
    int i = 0;
    for (const BDD& attractor : asyncLoops) {
        std::ofstream file(outputFile + "Attractor" + std::to_string(i) + ".csv");
        file << header << std::endl;
        file << prettyPrint(attractor) << std::endl;
        i++;
    }

    return 0;
}
