#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Diagnostics;

namespace Bounded_Async_Translator
{

    class Translator
    {
        static Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>> _nassigns; // next assignments
        static Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>> _iassigns; // init assignments
        static Dictionary<Ast.smv_module, List<Ast.expr>> _init; // init section
        static Dictionary<Ast.smv_module, List<Ast.expr>> _trans; // pransition relations
        static Dictionary<Ast.smv_module, List<string>> _params; // parameters
        static Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>> _svars; // set variables
        static Dictionary<Ast.smv_module, List<Tuple<string, Tuple<Int64, Int64>>>> _rvars; // range variables
        static Dictionary<Ast.smv_module, List<Tuple<string, List<List<string>>>>> _mvars; // module variables
        static Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>> _sbounded; // set variables
        static Dictionary<Ast.smv_module, List<Tuple<string, Tuple<Int64, Int64>>>> _rbounded; // range variables
        static Dictionary<Ast.smv_module, List<Tuple<string, List<List<string>>>>> _mbounded; // module variables
        static HashSet<Ast.smv_module> _modules;

        static Translator()
        {
        }
        public static void CreateTranslator(string fname)
        {
            //Instantiate Parser Object
            var parser = new Parser.SMV().parser_smv(fname);
            InitTables();
            //Create Module Name Table
            foreach (var mod in parser)
            {
                _modules.Add(mod);
            }
            //Create Tables of Module Sections
            CreateSectionTables(_modules);
            //Create Time Module : moduleName, NumberOfThreads, AsyncBoundNumber
            //Conjuct BoundedAsync transition with every transition relation of BOUNDED variables
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
            _mvars = new Dictionary<Ast.smv_module, List<Tuple<string, List<List<string>>>>>();

            _sbounded = new Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>>();
            _rbounded = new Dictionary<Ast.smv_module, List<Tuple<string, Tuple<long, long>>>>();
            _mbounded = new Dictionary<Ast.smv_module, List<Tuple<string, List<List<string>>>>>();


            _modules = new HashSet<Ast.smv_module>();
        }
        private static void CreateSectionTables(HashSet<Ast.smv_module> parsed)
        {
            CreateParamsTables(parsed);
            CreateInitTable(parsed);
            CreateTransTable(parsed);
            CreateVarTable(parsed);
            CreateBoundedTable(parsed);
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
                // list 
                foreach (var vars in modl.sections)
                {
                    if (vars.IsVar)
                    {
                        List<Tuple<string, List<string>>> svars = new List<Tuple<string, List<string>>>();
                        List<Tuple<string, Tuple<Int64, Int64>>> rvars = new List<Tuple<string, Tuple<long, long>>>();
                        List<Tuple<string, List<List<string>>>> mvars = new List<Tuple<string, List<List<string>>>>();

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
                                            Tuple<string, List<List<string>>> varmodlist = new Tuple<string, List<List<string>>>(moduletype.Item1, argslists);
                                            mvars.Add(varmodlist);
                                        }
                                        else
                                        {
                                            //No arguments
                                            List<List<string>> argslists = new List<List<string>>();
                                            Tuple<string, List<List<string>>> varmodlist = new Tuple<string, List<List<string>>>(moduletype.Item1, argslists);
                                            mvars.Add(varmodlist);
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
        private static void CreateBoundedTable(HashSet<Ast.smv_module> parsed)
        {
            foreach (var modl in parsed)
            {
                // list 
                foreach (var vars in modl.sections)
                {
                    if (vars.IsBounded)
                    {
                        List<Tuple<string, List<string>>> sbounded = new List<Tuple<string, List<string>>>();
                        List<Tuple<string, Tuple<Int64, Int64>>> rbounded = new List<Tuple<string, Tuple<long, long>>>();
                        List<Tuple<string, List<List<string>>>> mbounded = new List<Tuple<string, List<List<string>>>>();

                        var varlst = vars as Ast.section.Bounded;
                        foreach (var v in varlst.Item)
                        {
                            if (v.Item2.IsRange)
                            {
                                // type is Range
                                var moduletype = v.Item2 as Ast.types.Range;
                                Tuple<Int64, Int64> rangevalue = new Tuple<Int64, Int64>(moduletype.Item1, moduletype.Item2);
                                Tuple<string, Tuple<Int64, Int64>> varrngvl = new Tuple<string, Tuple<long, long>>(v.Item1, rangevalue);
                                rbounded.Add(varrngvl);
                            }
                            else
                            {
                                if (v.Item2.IsSet)
                                {//type is Set  
                                    var moduletype = v.Item2 as Ast.types.Set;
                                    List<string> setvals = new List<string>();
                                    foreach (var stvl in moduletype.Item)
                                    {
                                        setvals.Add(stvl);
                                    }
                                    Tuple<string, List<string>> varsvl = new Tuple<string, List<string>>(v.Item1, setvals);
                                    sbounded.Add(varsvl);
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
                                            Tuple<string, List<List<string>>> varmodlist = new Tuple<string, List<List<string>>>(moduletype.Item1, argslists);
                                            mbounded.Add(varmodlist);
                                        }
                                        else
                                        {
                                            //No arguments
                                            List<List<string>> argslists = new List<List<string>>();
                                            Tuple<string, List<List<string>>> varmodlist = new Tuple<string, List<List<string>>>(moduletype.Item1, argslists);
                                            mbounded.Add(varmodlist);
                                        }
                                    }
                                }
                            }
                        }
                        _mbounded.Add(modl, mbounded);
                        _rbounded.Add(modl, rbounded);
                        _sbounded.Add(modl, sbounded);
                    }
                }
            }
        }

     static private void CreateTimeModule(){

     
     }
     static private void AddBoundedAsync() { 
     
     }

    }
    class ExecuteTranslator
    {

        static void Main(string[] args)
        {
            Translator.CreateTranslator("mutant.smv");
            System.Console.WriteLine("adsadasdad");
        }

    }
}
