// Copyright (c) Microsoft Research 2017
// License: MIT. See LICENSE

#include "stdafx.h"
#include "Attractors.h"

extern "C" __declspec(dllexport) int attractors(int numVars, int ranges[], int minValues[], int numInputs[], int inputVars[], int numUpdates[],
    int inputValues[], int outputValues[], const char *output, int outputLength, const char *csvHeader, int headerLength, int mode,
    const char *initialCsvFilename, int initialCsvFilenameLength) {
    std::string initialFile(initialCsvFilename, initialCsvFilenameLength);
    std::string outputFile(output, outputLength);
    std::string header(csvHeader, headerLength);
    std::vector<int> rangesV(ranges, ranges + numVars);
    std::vector<int> minValuesV(minValues, minValues + numVars);
    std::vector<std::vector<int>> inputVarsV;
    std::vector<std::vector<int>> outputValuesV;
    std::vector<std::vector<std::vector<int>>> inputValuesV;

    int k = 0;
    for (int i = 0; i < numVars; i++) {
        std::vector<int> in;
        for (int j = 0; j < numInputs[i]; j++) {
            in.push_back(inputVars[k]);
            k++;
        }
        inputVarsV.push_back(in);
    }

    k = 0;
    for (int i = 0; i < numVars; i++) {
        std::vector<int> out;
        for (int j = 0; j < numUpdates[i]; j++) {
            out.push_back(outputValues[k]);
            k++;
        }
        outputValuesV.push_back(out);
    }

    k = 0;
    for (int i = 0; i < numVars; i++) {
        std::vector<std::vector<int>> in;
        for (int j = 0; j < numUpdates[i]; j++) {
            std::vector<int> v;
            for (int l = 0; l < numInputs[i]; l++) {
                v.push_back(inputValues[k]);
                k++;
            }
            in.push_back(v);
        }
        inputValuesV.push_back(in);
    }

    QNTable qn = QNTable(std::move(inputVarsV), std::move(inputValuesV), std::move(outputValuesV));
    Attractors a(std::move(minValuesV), std::move(rangesV), std::move(qn));
    BDD initialStates = a.readStatesFromCsv(initialFile);

    if (mode == 0) return a.runSync(initialStates, outputFile, header);
    return a.runAsync(initialStates, outputFile, header);
}