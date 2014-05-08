#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Bounded_Async_Translator
{
    
    class Translator
    {
        static Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>> _nassigns;
        static Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>> _iassigns;
        static Dictionary<Ast.smv_module, Ast.expr> _init;
        static Dictionary<Ast.smv_module, Ast.expr> _trans;
        static Dictionary<Ast.smv_module, List<string>> _params;
        static Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>> _svars; // set variables
        static Dictionary<Ast.smv_module, List<Tuple<string, Tuple<Int64, Int64>>>> _rvars; // range variables
        static Dictionary<Ast.smv_module, List<Tuple<string, List<List<string>>>>> _mvars; // module variables

     //   static Dictionary<Ast.smv_module, List<Tuple<Int64, Int64>>> _bounded; 
        static HashSet<Ast.smv_module> _modules; // (TODO: Check whether Ast.smv_module object can be used as key)

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
        }
        private static void InitTables() {
            _nassigns = new Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>>();
            _iassigns = new Dictionary<Ast.smv_module, List<Tuple<string, Ast.expr>>>();
            _init = new Dictionary<Ast.smv_module, Ast.expr>();
            _trans = new Dictionary<Ast.smv_module, Ast.expr>();
            _params = new Dictionary<Ast.smv_module, List<string>>();
            _svars = new Dictionary<Ast.smv_module, List<Tuple<string, List<string>>>>();
            _rvars = new Dictionary<Ast.smv_module, List<Tuple<string, Tuple<long, long>>>>();
            _mvars = new Dictionary<Ast.smv_module, List<Tuple<string, List<List<string>>>>>();
            //bounded to add
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
#if DEBUG
                System.Console.WriteLine("module name" + modl.name);

#endif
                foreach (var trns in modl.sections)
                {
                    if (trns.IsTrans)
                    {
                        var trs = trns as Ast.section.Trans;
                        _trans.Add(modl, trs.Item);
#if DEBUG
                        System.Console.WriteLine("transition" + trs.Item);
#endif
                    }
                }
            }
        }
        private static void CreateInitTable(HashSet<Ast.smv_module> parsed)
        {
            foreach (var modl in parsed)
            {
                foreach (var initial in modl.sections)
                {
                    if (initial.IsInit)
                    {
                        var init = initial as Ast.section.Init;
                        _init.Add(modl, init.Item);
                    }
                }
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
            // 
            
        }
        private static void CreateAssignsTable(HashSet<Ast.smv_module> parsed)
        {

        }

        static void Main(string[] args)
        {
            Cret
            
        }
    }
}
