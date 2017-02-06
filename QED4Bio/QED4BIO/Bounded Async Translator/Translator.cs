// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Diagnostics;
using Microsoft.FSharp.Collections;

namespace Bounded_Async_Translator
{

    class Translator
    {
        static Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>> _nassigns; // next assignments
        static Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>> _iassigns; // init assignments
        static Dictionary<Ast.smv_module, List<Ast.expr>> _init; // init section
        static Dictionary<Ast.smv_module, List<Ast.expr>> _trans; // transition relations
        static Dictionary<Ast.smv_module, List<string>> _params; // parameters
        static Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>> _svars; // set variables
        static Dictionary<Ast.smv_module, List<Tuple<string, Tuple<Int64, Int64>>>> _rvars; // range variables
        static Dictionary<Ast.smv_module, List<Tuple<string, Tuple<string, List<List<string>>>>>> _mvars; // module variables
        static HashSet<Ast.smv_module> _modules;
        static Dictionary<Ast.smv_module, List<string>> _bmodules;
        static long _asyncbound;
        static long _nthread;
        static long _clockbound;
        static Translator()
        {
        }
        public static string CreateTranslator(string fname, string cfname, Int64 numthread, Int64 asyncbound, Int64 clockbound)
        {
            //Instantiate Parser Object
            var parser = new Parser.SMV().parser_smv(fname);
            InitTables();
            //Create Module Name Table
            foreach (var mod in parser)
            {
                _modules.Add(mod);
            }

            CreateSectionTables(_modules);
            _nthread = numthread;
            _asyncbound = asyncbound;
            _clockbound = clockbound;
            _modules.Add(CreateTimeModule("timer"));
            _modules.Add(CreateClockModule("clock"));
            SetConfiguration(cfname);
            IntroduceAsyncBound();
            List<Ast.smv_module> asynbndintroduced = GenSmvModlsFromMem();
            return FlushModules(asynbndintroduced, fname);
        }
        private static void SetConfiguration(string cfname)
        {
            StreamReader r = new StreamReader(cfname);
            string line;
            List<string> lines = new List<string>();
            while ((line = r.ReadLine()) != null)
            {
                lines.Add(line);
            }
            List<Tuple<string, List<string>>> bmodules = GetBoundedModules(lines);
            foreach (var module in bmodules)
            {
                foreach (var modl in _modules)
                {
                    if (modl.name == module.Item1)
                    {
                        _bmodules.Add(modl, module.Item2);
                    }
                }
            }
        }
        private static void IntroduceAsyncBound()
        {
            int counter = 0;
            foreach (var modl in _modules)
            {

                if (_bmodules.ContainsKey(modl))
                {
                    if (modl.name == "main")
                    {
                        // foreach variable in bounded variable set of this module is constructor and needs to be introduced with parameter
                        List<List<string>> timervarlist = new List<List<string>>();
                        Tuple<string, List<List<string>>> timerconstvartupl = new Tuple<string, List<List<string>>>("timer", timervarlist);
                        Tuple<string, Tuple<string, List<List<string>>>> timerident = new Tuple<string, Tuple<string, List<List<string>>>>("t", timerconstvartupl);
                        _mvars[modl].Add(timerident);
                        List<List<string>> clockvarlist = new List<List<string>>();
                        Tuple<string, List<List<string>>> clockconstvartupl = new Tuple<string, List<List<string>>>("clock", clockvarlist);
                        Tuple<string, Tuple<string, List<List<string>>>> clockident = new Tuple<string, Tuple<string, List<List<string>>>>("c", clockconstvartupl);
                        _mvars[modl].Add(clockident);
                        foreach (var boundvar in _bmodules[modl])
                        {
                            foreach (var mvar in _mvars[modl])
                            {
                                if (boundvar == mvar.Item1)
                                {
                                    if (mvar.Item2.Item1 == "clock")
                                    {
                                        List<string> mtimervar = new List<string>();
                                        mtimervar.Add("t");
                                        mtimervar.Add("reset");
                                        mvar.Item2.Item2.Add(mtimervar);
                                    }
                                    else
                                    {
                                        if (mvar.Item2.Item1 == "Anchorcell") {
                                            List<string> mtimervar = new List<string>();
                                            mtimervar.Add("t");
                                            mtimervar.Add("reset");
                                            mvar.Item2.Item2.Add(mtimervar);
                                        }
                                        else
                                        {
                                            if (mvar.Item2.Item1 == "VPC") {

                                                List<string> mtimervar = new List<string>();
                                                List<string> clockvar = new List<string>();

                                                Debug.Assert(counter < _nthread);

                                                clockvar.Add("c");
                                                clockvar.Add("time");
                                                mvar.Item2.Item2.Add(clockvar);

                                                mtimervar.Add("t");
                                                mtimervar.Add("var" + counter.ToString());
                                                mvar.Item2.Item2.Add(mtimervar);
                                                counter++;  
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Modify clock
                        // put timerparameter to module's declaration
                        if (modl.name == "clock")
                        {
                            _params[modl].Add("reset");
                            Ast.expr rst = Ast.expr.NewIdent("reset");
                            Ast.expr resetident = Ast.expr.NewEq(rst, Ast.expr.NewInt(1));
                            //Ast.expr resetnident = Ast.expr.NewNext(rst);
                            //Ast.expr resetnidenttru = Ast.expr.NewEq(resetnident, Ast.expr.NewInt(1));
                            //Ast.expr takestep = Ast.expr.NewAnd(resetident, resetnidenttru);

                            for (int i = 0; i < _nassigns[modl].Count; i++)
                            {
                                Ast.expr.Cases nassign = _nassigns[modl][i].Item2 as Ast.expr.Cases;
                                Debug.Assert(nassign.IsCases);
                                List<Tuple<Ast.expr, Ast.expr>> caselist = new List<Tuple<Ast.expr, Ast.expr>>();
                                for (int j = 0; j < nassign.Item.Length; j++)
                                {
                                    if (nassign.Item[j].Item1.ToString() != "TRUE")
                                    {
                                        Ast.expr asyncupdated = Ast.expr.NewAnd(resetident, nassign.Item[j].Item1);
                                        FSharpList<Tuple<Ast.expr, Ast.expr>> exprlst = FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(nassign.Item);
                                        caselist.Add(new Tuple<Ast.expr, Ast.expr>(asyncupdated, exprlst[j].Item2));
                                    }
                                    else
                                    {
                                        Ast.expr asyncupdated = nassign.Item[j].Item1;
                                        FSharpList<Tuple<Ast.expr, Ast.expr>> exprlst = FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(nassign.Item);
                                        caselist.Add(new Tuple<Ast.expr, Ast.expr>(asyncupdated, exprlst[j].Item2));
                                    }
                                }
                                Tuple<string, Ast.expr> asyncupdatedassign = new Tuple<string, Ast.expr>(_nassigns[modl][i].Item1,
                                   Ast.expr.NewCases(FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(caselist)));
                                _nassigns[modl][i] = asyncupdatedassign;
                            }
                        }                         
                        else
                        {
                            if (modl.name == "Anchorcell")
                            {
                                _params[modl].Add("reset");
                                Ast.expr rst = Ast.expr.NewIdent("reset");
                                Ast.expr resetident = Ast.expr.NewEq(rst, Ast.expr.NewInt(0));
                                Ast.expr resetnident = Ast.expr.NewNext(rst);
                                Ast.expr resetnidenttru = Ast.expr.NewEq(resetnident, Ast.expr.NewInt(1));
                                Ast.expr takestep = Ast.expr.NewAnd(resetident, resetnidenttru);
                                
                                for (int i = 0; i < _nassigns[modl].Count; i++)
                                {
                                    Ast.expr.Cases nassign = _nassigns[modl][i].Item2 as Ast.expr.Cases;
                                    Debug.Assert(nassign.IsCases);
                                    List<Tuple<Ast.expr, Ast.expr>> caselist = new List<Tuple<Ast.expr, Ast.expr>>();
                                    for (int j = 0; j < nassign.Item.Length; j++)
                                    {
                                        if (nassign.Item[j].Item1.ToString() != "TRUE")
                                        {
                                            Ast.expr asyncupdated = Ast.expr.NewAnd(takestep, nassign.Item[j].Item1);
                                            FSharpList<Tuple<Ast.expr, Ast.expr>> exprlst = FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(nassign.Item);
                                            caselist.Add(new Tuple<Ast.expr, Ast.expr>(asyncupdated, exprlst[j].Item2));
                                        }
                                        else
                                        {
                                            Ast.expr asyncupdated = nassign.Item[j].Item1;
                                            FSharpList<Tuple<Ast.expr, Ast.expr>> exprlst = FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(nassign.Item);
                                            caselist.Add(new Tuple<Ast.expr, Ast.expr>(asyncupdated, exprlst[j].Item2));
                                        }
                                    }
                                    Tuple<string, Ast.expr> asyncupdatedassign = new Tuple<string, Ast.expr>(_nassigns[modl][i].Item1,
                                       Ast.expr.NewCases(FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(caselist)));
                                    _nassigns[modl][i] = asyncupdatedassign;
                                }                          
                            }                           
                            else {
                                _params[modl].Add("time");
                                _params[modl].Add("t");
                                string asyncboundparam = _params[modl][_params[modl].Count - 1];
                                Ast.expr asyncident = Ast.expr.NewIdent(asyncboundparam);
                                Ast.expr nasyncindent = Ast.expr.NewNext(asyncident);
                                Ast.expr takestep = Ast.expr.NewLt(asyncident, nasyncindent);
                                // foreach bounded variable belongs to this module need to be introduce with async bound in their next assignments
                                for (int i = 0; i < _nassigns[modl].Count; i++)
                                {
                                    Ast.expr.Cases nassign = _nassigns[modl][i].Item2 as Ast.expr.Cases;
                                    Debug.Assert(nassign.IsCases);
                                    List<Tuple<Ast.expr, Ast.expr>> caselist = new List<Tuple<Ast.expr, Ast.expr>>();
                                    for (int j = 0; j < nassign.Item.Length; j++)
                                    {
                                        if (nassign.Item[j].Item1.ToString() != "TRUE")
                                        {
                                            Ast.expr asyncupdated = Ast.expr.NewAnd(takestep, nassign.Item[j].Item1);
                                            FSharpList<Tuple<Ast.expr, Ast.expr>> exprlst = FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(nassign.Item);
                                            caselist.Add(new Tuple<Ast.expr, Ast.expr>(asyncupdated, exprlst[j].Item2));
                                        }
                                        else
                                        {
                                            Ast.expr asyncupdated = nassign.Item[j].Item1;
                                            FSharpList<Tuple<Ast.expr, Ast.expr>> exprlst = FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(nassign.Item);
                                            caselist.Add(new Tuple<Ast.expr, Ast.expr>(asyncupdated, exprlst[j].Item2));
                                        }
                                    }
                                    Tuple<string, Ast.expr> asyncupdatedassign = new Tuple<string, Ast.expr>(_nassigns[modl][i].Item1,
                                         Ast.expr.NewCases(FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(caselist)));
                                    _nassigns[modl][i] = asyncupdatedassign;
                                }                   
                           }
                        }
                    }
                }
            }
        }

        private static List<Ast.smv_module> GenSmvModlsFromMem()
        {
            List<Ast.smv_module> astmodules = new List<Ast.smv_module>();
            foreach (var modl in _modules)
            {
                List<Ast.section> sections = new List<Ast.section>();
                List<Tuple<string, Ast.types>> vrs = new List<Tuple<string, Ast.types>>();
                //svars
                List<Tuple<string, Ast.types>> svars = ConvertSetTupleToTypes(_svars[modl]);
                foreach (var v in svars)
                {
                    vrs.Add(v);
                }
                // Ast.section svarsec = Ast.section.NewVar(FSharpInteropExtensions.ToFSharplist<Tuple<string, Ast.types>> (svars));
                //rvars
                List<Tuple<string, Ast.types>> rvars = ConvertRngTupleToTypes(_rvars[modl]);
                foreach (var v in rvars)
                {
                    vrs.Add(v);
                } 
                //mvars
                List<Tuple<string, Ast.types>> mvars = ConvertModlTupleToTypes(_mvars[modl]);
                foreach (var v in mvars)
                {
                    vrs.Add(v);
                }
                Ast.section varsec = Ast.section.NewVar(FSharpInteropExtensions.ToFSharplist<Tuple<string, Ast.types>>(vrs));
                sections.Add(varsec);
                //init
                foreach (var init in _init[modl])
                {
                    sections.Add(Ast.section.NewInit(init));
                }
                //trans
                foreach (var trans in _trans[modl])
                {
                    sections.Add(Ast.section.NewTrans(trans));
                }

                List<Ast.assign> assigns = new List<Ast.assign>();
                using (var e1 = _iassigns[modl].GetEnumerator())
                {
                    while (e1.MoveNext())
                    {
                        var iassign = e1.Current;
                        Ast.assign iasgn = Ast.assign.NewInitAssign(iassign.Item1, iassign.Item2);
                        assigns.Add(iasgn);
                    }                
                }

                //assigns Bug is here 
                using (var e2 = _nassigns[modl].GetEnumerator())
                {

                    while ( e2.MoveNext())
                    {
                        var nassign = e2.Current;
                        Ast.assign nasgn = Ast.assign.NewNextAssign(nassign.Item1, nassign.Item2);
                        assigns.Add(nasgn);
                    }
                }

                /*
                foreach (var iassign in _iassigns[modl])
                {
                    Ast.assign asgn = Ast.assign.NewInitAssign(iassign.Item1, iassign.Item2);
                    assigns.Add(asgn);
                }
                foreach (var nassign in _nassigns[modl])
                {
                    Ast.assign asgn = Ast.assign.NewNextAssign(nassign.Item1, nassign.Item2);
                    assigns.Add(asgn);
                }*/
                if (assigns.Count > 0)
                {
                    sections.Add(Ast.section.NewAssigns(FSharpInteropExtensions.ToFSharplist<Ast.assign>(assigns)));
                }
                Ast.smv_module asyncboundedmodl = new Ast.smv_module(modl.name, FSharpInteropExtensions.ToFSharplist<string>(_params[modl]) /*modl.parameters*/, FSharpInteropExtensions.ToFSharplist<Ast.section>(sections));
                astmodules.Add(asyncboundedmodl);
            }
            return astmodules;
        }
        private static string FlushModules(List<Ast.smv_module> modules, string fname)
        {
            string[] splitf = fname.Split('.');
            string genfname = splitf[0] + "_async.smv";
            StreamWriter writer = new StreamWriter(genfname);
            foreach (var modl in modules)
            {
                writer.WriteLine(modl.ToString());
            }
            writer.Close();
            return genfname;
        }
        private static List<Tuple<string, List<string>>> GetBoundedModules(List<string> conflines)
        {
            List<Tuple<string, List<string>>> bmodules = new List<Tuple<string, List<string>>>();
            foreach (string l in conflines)
            {
                string[] words = l.Split(' ');
                List<string> varnames = new List<string>();
                for (int i = 2; i < words.Length; i++)
                {
                    varnames.Add(words[i]);
                }
                Tuple<string, List<string>> modlvars = new Tuple<string, List<string>>(words[1], varnames);
                bmodules.Add(modlvars);
            }
            return bmodules;
        }
        private static void InitTables()
        {
            _nassigns = new Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>>();
            _iassigns = new Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>>();
            _init = new Dictionary<Ast.smv_module, List<Ast.expr>>();
            _trans = new Dictionary<Ast.smv_module, List<Ast.expr>>();
            _params = new Dictionary<Ast.smv_module, List<string>>();
            _svars = new Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>>();
            _rvars = new Dictionary<Ast.smv_module, List<Tuple<string, Tuple<long, long>>>>();
            _mvars = new Dictionary<Ast.smv_module, List<Tuple<string, Tuple<string, List<List<string>>>>>>();
            _bmodules = new Dictionary<Ast.smv_module, List<string>>();
            _modules = new HashSet<Ast.smv_module>();
        }
        private static void CreateSectionTables(HashSet<Ast.smv_module> parsed)
        {
            CreateParamsTables(parsed);
            CreateInitTable(parsed);
            CreateTransTable(parsed);
            CreateVarTable(parsed);
            CreateAssignsTable(parsed);
        }
        private static void CreateAssignsTable(HashSet<Ast.smv_module> parsed)
        {
            foreach (var modl in parsed)
            {  
                List<Tuple<string, Ast.expr>> iassigns = new List<Tuple<string, Ast.expr>>();
                List<Tuple<string, Ast.expr>> nassigns = new List<Tuple<string, Ast.expr>>();
                foreach (var assignsec in modl.sections)
                {
                    if (assignsec.IsAssigns)
                    {
                        var assignlist = assignsec as Ast.section.Assigns;
                        foreach (var assign in assignlist.Item)
                        {
                            var assigntyped = assign as Ast.assign;
                            if (assigntyped.IsInitAssign)
                            {
                                var assigninit = assigntyped as Ast.assign.InitAssign;
                                Tuple<string, Ast.expr> varassigninit = new Tuple<string, Ast.expr>(assigninit.Item1, assigninit.Item2);
                                iassigns.Add(varassigninit);

                            }
                            else
                            {                               
                                Debug.Assert(assigntyped.IsNextAssign);
                                var assingnext = assigntyped as Ast.assign.NextAssign;
                                Tuple<string, Ast.expr> varassignnext = new Tuple<string, Ast.expr>(assingnext.Item1, assingnext.Item2);
                                nassigns.Add(varassignnext);                                                          
                            }
                        }
                    }
                }
                _iassigns.Add(modl, iassigns);
                _nassigns.Add(modl, nassigns);
            }
        }

        private static void CreateParamsTables(HashSet<Ast.smv_module> parsed)
        {
            // Create Params Table
            foreach (var modl in parsed)
            {
                List<string> pars = new List<string>();
                foreach (string para in modl.parameters)
                {
                    pars.Add(para);
                }
                _params.Add(modl, pars);
            }
        }
        private static void CreateTransTable(HashSet<Ast.smv_module> parsed)
        {
            foreach (var modl in parsed)
            {
                List<Ast.expr> translist = new List<Ast.expr>();
                foreach (var trns in modl.sections)
                {
                    if (trns.IsTrans)
                    {
                        var trs = trns as Ast.section.Trans;
                        translist.Add(trs.Item);
                    }
                }
                _trans.Add(modl, translist);
            }
        }
        private static void CreateInitTable(HashSet<Ast.smv_module> parsed)
        {
            foreach (var modl in parsed)
            {
                List<Ast.expr> initlist = new List<Ast.expr>();
                foreach (var initial in modl.sections)
                {
                    if (initial.IsInit)
                    {
                        var init = initial as Ast.section.Init;
                        initlist.Add(init.Item);
                    }
                }
                _init.Add(modl, initlist);
            }
        }
        private static void CreateVarTable(HashSet<Ast.smv_module> parsed)
        {
            foreach (var modl in parsed)
            {
                foreach (var vars in modl.sections)
                {
                    if (vars.IsVar)
                    {
                        List<Tuple<string, List<string>>> svars = new List<Tuple<string, List<string>>>();
                        List<Tuple<string, Tuple<Int64, Int64>>> rvars = new List<Tuple<string, Tuple<long, long>>>();
                        List<Tuple<string, Tuple<string, List<List<string>>>>> mvars = new List<Tuple<string, Tuple<string, List<List<string>>>>>();

                        var varlst = vars as Ast.section.Var;
                        foreach (var v in varlst.Item)
                        {
                            if (v.Item2.IsRange)
                            {
                                // type is Range
                                var moduletype = v.Item2 as Ast.types.Range;
                                Tuple<Int64, Int64> rangevalue = new Tuple<Int64, Int64>(moduletype.Item1, moduletype.Item2);
                                Tuple<string, Tuple<Int64, Int64>> varrngvl = new Tuple<string, Tuple<long, long>>(v.Item1, rangevalue);
                                rvars.Add(varrngvl);
                            }
                            else
                            {
                                if (v.Item2.IsSet)
                                {//type is Set  
                                    var moduletype = v.Item2 as Ast.types.Set;
                                    // foreach element in the list
                                    List<string> setvals = new List<string>();
                                    foreach (var stvl in moduletype.Item)
                                    {
                                        setvals.Add(stvl);
                                    }
                                    Tuple<string, List<string>> varsvl = new Tuple<string, List<string>>(v.Item1, setvals);
                                    svars.Add(varsvl);
                                }
                                else
                                {
                                    if (v.Item2.IsModule)
                                    {
                                        //type is Module
                                        var moduletype = v.Item2 as Ast.types.Module;
                                        var modulename = moduletype.Item1;
                                        var moduleargs = moduletype.Item2;

                                        if (Microsoft.FSharp.Core.OptionModule.IsSome(moduleargs))
                                        {   //
                                            //Has arguments
                                            var argumentslists = moduleargs.Value;
                                            List<List<string>> argslists = new List<List<string>>();
                                            foreach (var argumentlist in argumentslists)
                                            {
                                                List<string> arglst = new List<string>();
                                                foreach (var argument in argumentlist)
                                                {
                                                    arglst.Add(argument);
                                                }
                                                argslists.Add(arglst);
                                            }
                                            Tuple<string, List<List<string>>> varmodlist = new Tuple<string, List<List<string>>>(modulename, argslists);
                                            Tuple<string, Tuple<string, List<List<string>>>> identwithvar = new Tuple<string, Tuple<string, List<List<string>>>>(v.Item1, varmodlist);
                                            mvars.Add(identwithvar);
                                        }
                                        else
                                        {
                                            //No arguments
                                            List<List<string>> argslists = new List<List<string>>();
                                            Tuple<string, List<List<string>>> varmodlist = new Tuple<string, List<List<string>>>(modulename, argslists);
                                            Tuple<string, Tuple<string, List<List<string>>>> identwithvar = new Tuple<string, Tuple<string, List<List<string>>>>(v.Item1, varmodlist);
                                            mvars.Add(identwithvar);
                                        }
                                    }
                                }
                            }
                        }
                        _mvars.Add(modl, mvars);
                        _rvars.Add(modl, rvars);
                        _svars.Add(modl, svars);
                    }
                }
            }
        }

        private static List<Tuple<string, Tuple<Int64, Int64>>> TimerVars()
        {
            string variable = "var";
            List<Tuple<string, Tuple<Int64, Int64>>> variables = new List<Tuple<string, Tuple<Int64, Int64>>>();
            for (int i = 0; i < _nthread; i++)
            {
                string varstr = variable + i.ToString();
                Tuple<Int64, Int64> varrange = new Tuple<Int64, Int64>(0, _asyncbound);
                Tuple<string, Tuple<Int64, Int64>> varrangtup = new Tuple<string, Tuple<Int64, Int64>>(varstr, varrange);
                variables.Add(varrangtup);
            }
            string rststr = "reset";
            Tuple<Int64, Int64> varrst = new Tuple<Int64, Int64>(0, 1);
            Tuple<string, Tuple<Int64, Int64>> varrangtuprst = new Tuple<string, Tuple<Int64, Int64>>(rststr, varrst);
            variables.Add(varrangtuprst);
            return variables;
        }
        private static List<Tuple<string, Tuple<Int64, Int64>>> ClockVars()
        {
            string variable = "time";
            List<Tuple<string, Tuple<Int64, Int64>>> variables = new List<Tuple<string, Tuple<long, long>>>();
            Tuple<Int64, Int64> timerng = new Tuple<long, long>(0, _clockbound);
            Tuple<string, Tuple<Int64, Int64>> vartimerng = new Tuple<string, Tuple<long, long>>(variable, timerng);
            variables.Add(vartimerng);
            return variables;
        }
        private static Ast.expr TimerInit(List<Tuple<string, Tuple<Int64, Int64>>> variables)
        {
            List<Ast.expr> identifiers = new List<Ast.expr>();
            foreach (var v in variables)
            {
                Ast.expr varident = Ast.expr.NewIdent(v.Item1);
                Ast.expr valident = Ast.expr.NewInt(v.Item2.Item1);
                Ast.expr valvareqident = Ast.expr.NewEq(varident, valident);
                identifiers.Add(valvareqident);
            }
            return ConjuctAll(identifiers);
            //return identifiers;
        }
        /*Framework:  
        //TR1. forall var:variable :: var == asyncbound && !reset --> next(var) == var
        //TR2. forall var:variable :: var == asyncbound && reset --> next(var)=0
        //TR3. forall var:variable :: next(var) == asyncbound --> next(reset)
        //TR4. exists var:variable :: next(var) != asyncbound --> !next(reset)      
        //TR5. forall var: variables : next(var) != var
        //TR6. forall var:variables :: var < asyncbound --> next(var) = var+1 | next(var) = var
         */
        private static Ast.expr TimerTransitions(List<Tuple<string, Tuple<Int64, Int64>>> variables, Ast.expr resetident)
        {
            List<Ast.expr> transrel = new List<Ast.expr>();
            List<Ast.expr> transrel1 = new List<Ast.expr>();
            List<Ast.expr> transrel2 = new List<Ast.expr>();
            List<Ast.expr> transrel3 = new List<Ast.expr>();
            List<Ast.expr> transrel4 = new List<Ast.expr>();
            List<Ast.expr> transrel5 = new List<Ast.expr>();
            List<Ast.expr> transrel6 = new List<Ast.expr>();
            List<Ast.expr> varident = new List<Ast.expr>();
            List<Ast.expr> resetidents = new List<Ast.expr>();

            Ast.expr asyncbndident = Ast.expr.NewInt(_asyncbound);
            foreach (var v in variables)
            {
                if (v.Item1 != "reset")
                    varident.Add(Ast.expr.NewIdent(v.Item1));
                else
                    resetident = Ast.expr.NewIdent(v.Item1);
            }
            //TR1
            foreach (Ast.expr vident in varident)
            {
                transrel1.Add(Ast.expr.NewImp(Ast.expr.NewAnd(Ast.expr.NewEq(vident, asyncbndident), Ast.expr.NewEq(resetident, Ast.expr.NewInt(0))),
                                             Ast.expr.NewEq(vident, Ast.expr.NewNext(vident))));
            }
            transrel.Add(ConjuctAll(transrel1));
          //  transrel.Add(transrel1);
            //TR2
            foreach (Ast.expr vident in varident)
            {
                transrel2.Add(Ast.expr.NewImp(Ast.expr.NewAnd(Ast.expr.NewEq(vident, asyncbndident), Ast.expr.NewEq(resetident, Ast.expr.NewInt(1))),
                              Ast.expr.NewEq(Ast.expr.NewNext (vident), Ast.expr.NewInt(0))));
            }
            transrel.Add(ConjuctAll(transrel2));
            
            //TR3            
            //  transrel.Add(transrel2);
            // collect all variables = asyncbound
            List<Ast.expr> allvreachedbound = new List<Ast.expr>();
            foreach (Ast.expr vident in varident)
            {
                allvreachedbound.Add(Ast.expr.NewEq(Ast.expr.NewNext(vident), asyncbndident));                
            }
            Ast.expr allconjucted = ConjuctAll(allvreachedbound);
            //foreach (Ast.expr vident in varident)
            //{
                transrel3.Add(Ast.expr.NewImp(allconjucted, Ast.expr.NewEq(Ast.expr.NewNext(resetident), Ast.expr.NewInt(1))));
            //}

            transrel.Add(ConjuctAll(transrel3));
               
          //  transrel.Add(transrel3);
            //TR4
            List<Ast.expr> allvnotreachedbound = new List<Ast.expr>();
            foreach (Ast.expr vident in varident)
            {
                allvnotreachedbound.Add(Ast.expr.NewEq(Ast.expr.NewNext(vident), asyncbndident));
            }
            Ast.expr allnconjucted = ConjuctAll(allvnotreachedbound);
            Ast.expr notreached = Ast.expr.NewNot(allnconjucted);
            //foreach (Ast.expr vident in varident)
            //{
                transrel4.Add(Ast.expr.NewImp(notreached, Ast.expr.NewEq(Ast.expr.NewNext(resetident), Ast.expr.NewInt(0))));
            //}
            transrel.Add(ConjuctAll(transrel4));
            //TR5
            foreach (Ast.expr vident in varident)
            {
                transrel5.Add(Ast.expr.NewNeq(Ast.expr.NewNext(vident), vident));
            }
            transrel.Add(DisjunctAll(transrel5));

            //TR6
            foreach (Ast.expr vident in varident)
            {
                transrel6.Add(Ast.expr.NewImp(Ast.expr.NewLt(vident, asyncbndident), Ast.expr.NewOr(Ast.expr.NewEq(Ast.expr.NewNext(vident), Ast.expr.NewAdd(vident, Ast.expr.NewInt(1))),
                                               Ast.expr.NewEq(Ast.expr.NewNext(vident), vident))));
            }
            transrel.Add(ConjuctAll(transrel6));
            return ConjuctAll(transrel);
        }
        private static Ast.expr ConjuctAll(List<Ast.expr> transitions)
        {
            Ast.expr[] conjuctions = transitions.ToArray();
            Ast.expr allconjucted = conjuctions[0];

            for (int i = 1; i < conjuctions.Length; i++)
            {
                allconjucted = Ast.expr.NewAnd(allconjucted, conjuctions[i]);
            }
            return allconjucted;
        } // Take care of one extra conj.
        private static Ast.expr DisjunctAll(List<Ast.expr> transitions ){
        
            Ast.expr[] disjunctions = transitions.ToArray();
            Ast.expr alldisjuncted = disjunctions[0];

            for (int i = 1; i < disjunctions.Length; i++)
            {
                alldisjuncted = Ast.expr.NewOr(alldisjuncted, disjunctions[i]);
            }
            return alldisjuncted;
        
        }
        private static List<Tuple<string, Ast.types>> ConvertRngTupleToTypes(List<Tuple<string, Tuple<Int64, Int64>>> variables)
        {
            List<Tuple<string, Ast.types>> vartypes = new List<Tuple<string, Ast.types>>();
            foreach (var v in variables)
            {
                Tuple<string, Ast.types> vartypetup = new Tuple<string, Ast.types>(v.Item1, Ast.types.NewRange(v.Item2.Item1, v.Item2.Item2));
                vartypes.Add(vartypetup);
            }
            return vartypes;
        }
        private static List<Tuple<string, Ast.types>> ConvertModlTupleToTypes(List<Tuple<string, Tuple<string, List<List<string>>>>> variables)
        {
            List<Tuple<string, Ast.types>> vartypes = new List<Tuple<string, Ast.types>>();
            foreach (var v in variables)
            {
                var a = Ast.mkModuleType(v.Item2.Item1, v.Item2.Item2);
                Tuple<string, Ast.types> vartypetup = new Tuple<string, Ast.types>(v.Item1, Ast.types.NewModule(a.Item1, a.Item2));
                vartypes.Add(vartypetup);
            }
            return vartypes;
        }
        private static List<Tuple<string, Ast.types>> ConvertSetTupleToTypes(List<Tuple<string, List<string>>> variables)
        {
            List<Tuple<string, Ast.types>> vartypes = new List<Tuple<string, Ast.types>>();
            foreach (var v in variables)
            {
                Tuple<string, Ast.types> vartypetup = new Tuple<string, Ast.types>(v.Item1, Ast.types.NewSet(FSharpInteropExtensions.ToFSharplist<string>(v.Item2)));
                vartypes.Add(vartypetup);
            }
            return vartypes;
        }
        private static Tuple<Ast.section, Tuple<List<Tuple<string, Ast.expr>>, List<Tuple<string, Ast.expr>>>> ClockAssigns(List<Tuple<string, Tuple<Int64, Int64>>> variables)
        {
            List<Tuple<string, Ast.expr>> stringexprlstinit = new List<Tuple<string, Ast.expr>>();
            List<Tuple<string, Ast.expr>> stringexprlstnext = new List<Tuple<string, Ast.expr>>();
            Ast.expr clockindent = Ast.expr.NewInt(_clockbound);

            Ast.expr timerident = Ast.expr.NewIdent("time");
            Ast.expr inctimerident = Ast.expr.NewAdd(timerident, Ast.expr.NewInt(1));
            Ast.expr truident = Ast.expr.NewIdent("TRUE");
            Ast.expr zeroident = Ast.expr.NewInt(0);
            Ast.expr guard = Ast.expr.NewLt(timerident, Ast.expr.NewInt(_clockbound));
            // init 
            Ast.assign initident = Ast.assign.NewInitAssign("time", zeroident);
            // next
            List<Tuple<Ast.expr, Ast.expr>> caseslst = new List<Tuple<Ast.expr, Ast.expr>>();
            Tuple<Ast.expr, Ast.expr> guardcs = new Tuple<Ast.expr, Ast.expr>(guard, inctimerident);
            caseslst.Add(guardcs);
            stringexprlstinit.Add(new Tuple<string, Ast.expr>("time", zeroident));

            Tuple<Ast.expr, Ast.expr> trucs = new Tuple<Ast.expr, Ast.expr>(truident, timerident);
            caseslst.Add(trucs);
            Ast.expr caseident = Ast.expr.NewCases(FSharpInteropExtensions.ToFSharplist<Tuple<Ast.expr, Ast.expr>>(caseslst));
            Ast.assign nextident = Ast.assign.NewNextAssign("time", caseident);
            stringexprlstnext.Add(new Tuple<string, Ast.expr>("time", caseident));

            List<Ast.assign> assignlst = new List<Ast.assign>();
            assignlst.Add(initident);
            assignlst.Add(nextident);
            Ast.section assignsec = Ast.section.NewAssigns(FSharpInteropExtensions.ToFSharplist<Ast.assign>(assignlst));

            Tuple<Ast.section, Tuple<List<Tuple<string, Ast.expr>>, List<Tuple<string, Ast.expr>>>> sectionwithmemoryvalues =
                new Tuple<Ast.section, Tuple<List<Tuple<string, Ast.expr>>, List<Tuple<string, Ast.expr>>>>
               (assignsec, new Tuple<List<Tuple<string, Ast.expr>>, List<Tuple<string, Ast.expr>>>(stringexprlstinit, stringexprlstnext));
            return sectionwithmemoryvalues;
        }
        private static Ast.smv_module CreateClockModule(string name)
        {
            List<Tuple<string, Tuple<Int64, Int64>>> vars;
            List<Tuple<string, Ast.types>> varwithtpyes;
            List<string> parameters = new List<string>();
            List<Ast.section> sections = new List<Ast.section>();
            // Variable section
            vars = ClockVars();
            varwithtpyes = ConvertRngTupleToTypes(vars);
            Ast.section varsec = Ast.section.NewVar(FSharpInteropExtensions.ToFSharplist<Tuple<string, Ast.types>>(varwithtpyes));
            //Assignment section
            Tuple<Ast.section, Tuple<List<Tuple<string, Ast.expr>>, List<Tuple<string, Ast.expr>>>> getsections = ClockAssigns(vars);
            Ast.section assignsec = getsections.Item1;
            sections.Add(varsec); sections.Add(assignsec);
            //Create Module 
            Ast.smv_module clockmodl = new Ast.smv_module(name,
                                        FSharpInteropExtensions.ToFSharplist<string>(parameters),
                                        FSharpInteropExtensions.ToFSharplist<Ast.section>(sections));
            _iassigns[clockmodl] = getsections.Item2.Item1;
            _nassigns[clockmodl] = getsections.Item2.Item2;
            _params[clockmodl] = parameters;
            _init[clockmodl] = new List<Ast.expr>();
            _mvars[clockmodl] = new List<Tuple<string, Tuple<string, List<List<string>>>>>();
            _rvars[clockmodl] = vars;
            _svars[clockmodl] = new List<Tuple<string, List<string>>>();
            _trans[clockmodl] = new List<Ast.expr>();

            return clockmodl;

        }
        private static Ast.smv_module CreateTimeModule(string name)
        {
            List<Tuple<string, Tuple<Int64, Int64>>> vars;
            List<Tuple<string, Ast.types>> varwithtpyes;
            Ast.expr init;
            Ast.expr transrel;
            Ast.expr resetident = Ast.expr.NewIdent("reset");
            List<string> parameters = new List<string>();
            List<Ast.section> secs = new List<Ast.section>();
            //             
            vars = TimerVars();
            varwithtpyes = ConvertRngTupleToTypes(vars);
            init = TimerInit(vars);
            transrel = TimerTransitions(vars, resetident);
            Debug.Assert(transrel != null);
            // Create sections 
            Ast.section trans = Ast.section.NewTrans(transrel);
            Ast.section inits = Ast.section.NewInit(init);
            Ast.section varbls = Ast.section.NewVar(FSharpInteropExtensions.ToFSharplist<Tuple<string, Ast.types>>(varwithtpyes));
            // Dirty conversions            
            Microsoft.FSharp.Collections.FSharpList<Ast.section> sections = FSharpInteropExtensions.ToFSharplist<Ast.section>(secs);
            Microsoft.FSharp.Collections.FSharpList<string> pars = FSharpInteropExtensions.ToFSharplist<string>(parameters);
            // Create module
            Ast.smv_module timermodule = new Ast.smv_module(name, pars, sections);
            //Fill memory representation
            //Trans
            List<Ast.expr> transrels = new List<Ast.expr>();
            transrels.Add(transrel);
            _trans.Add(timermodule, transrels);
            //init
            List<Ast.expr> initials = new List<Ast.expr>();
            initials.Add(init);
            _init.Add(timermodule, initials);
            //params
            _params.Add(timermodule, parameters);
            //vars
            _rvars.Add(timermodule, vars);
            //_mvars
            _mvars.Add(timermodule, new List<Tuple<string, Tuple<string, List<List<string>>>>>());
            //_svars
            _svars.Add(timermodule, new List<Tuple<string, List<string>>>());
            //iassigns
            _iassigns.Add(timermodule, new List<Tuple<string, Ast.expr>>());
            //nassigns
            _nassigns.Add(timermodule, new List<Tuple<string, Ast.expr>>());
            return timermodule;
        }
    }
    public static class FSharpInteropExtensions
    {
        public static Microsoft.FSharp.Collections.FSharpList<TItemType> ToFSharplist<TItemType>(this IEnumerable<TItemType> myList)
        {
            return Microsoft.FSharp.Collections.ListModule.OfSeq<TItemType>(myList);
        }

        public static IEnumerable<TItemType> ToEnumerable<TItemType>(this Microsoft.FSharp.Collections.FSharpList<TItemType> fList)
        {
            return Microsoft.FSharp.Collections.SeqModule.OfList<TItemType>(fList);
        }
    }
    class ExecuteTranslator
    {
        static void Main(string[] args)
        {
            if (args.Length < 5 || args.Length > 5)
            {
                System.Console.WriteLine(" Usage : program.exe source.smv config.txt numthread asyncbound clockbound");
            }
            else
            {
                string[] arg0 = args[0].Split('.');
                string[] arg1 = args[0].Split('.');
                //string[] arg2 = args[0].Split('.');
                if (arg0[1] != "smv")
                {
                    System.Console.Error.WriteLine("Error : .smv source required");
                    return;
                }
                else
                {
                    string filegen = Translator.CreateTranslator(args[0], args[1], Int64.Parse(args[2]), Int64.Parse(args[3]), Int64.Parse(args[4]));
                    return;
                }
            }
            System.Console.WriteLine("");
            return;
        }
    }
}
