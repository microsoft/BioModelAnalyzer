module Tests
open Parser

let p = new SMV()

let testparser teststring = 
    try 
        let v = p.parser_smv_string teststring 
        printfn "Sucess: %O" (String.concat "\n-------------\n" (List.map string v))
    with ParseException(e) -> printfn "Failed %s" e

let test1 = "-- Model of the VPC system 
-- NuSMV 2.5.3
-- Tool download at http://nusmv.fbk.eu/

----------------------------------------------------------------

MODULE main

VAR
	mut : organiser;
	t   : timer;
	c   : clock;
	P3p : VPC(AC.ISd3, low, P4p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60,
	      mut.mpk1, mut.dep1, mut.lst, t.var1, c.time); 
	P4p : VPC(AC.ISd2, P3p.LS, P5p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var2, c.time);
	P5p : VPC(AC.ISd1, P4p.LS, P6p.LS, mut.lin12, 
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var3, c.time);
	P6p : VPC(AC.ISd0, P5p.LS, P7p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var4, c.time);
	P7p : VPC(AC.ISd1, P6p.LS, P8p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var5, c.time);
	P8p : VPC(AC.ISd2, P7p.LS, low, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var6, c.time);
	AC  : Anchorcell(mut.ac, mut.lin3, mut.lin15);

----------------------------------------------------------------"

let test2 = "MODULE VPC(IS, LSleft, LSright, 	v_lin12, v_let23, v_sem5,
	      v_let60, v_mpk1, v_dep1, v_lst, go, time)

VAR
     lst   : {off, low, med, high};
	LS    : {low, med, high};
     sur2  : {off, low, med, high};
     let23 : {off, low, low1, med, high};
	sem5  : {off, low, low1, med, high};
	let60 : {off, low, med, high};
	mpk1  : {off, low, med, high};
	lin12 : {off, low, med, high};
	dep1  : {off, low, med};
	
	fate      : {af, primary, secondary, tertiary, mixed};
	cellcycle : {G1phase, Sphase, G2phase};
	counter   : 0..11;

ASSIGN

------lst-----
	init(lst) :=
		  case
			v_lst = ko : off;
			v_lst = wt : low;
		  esac;
	next(lst) :=
		  case
			(!go & next(go)) & lst != off & mpk1 = high : low; 
			(!go & next(go)) & ((lst = low & lin12 = med & mpk1 != high & mpk1 != med)
			| (lst = high & mpk1 = high)) : med; 
			(!go & next(go)) & (lst = low | lst = med) & 
			lin12 = high & mpk1 != high  : high;
			TRUE : lst; 
		  esac;

-----LS-----
	init(LS) := low;
	next(LS) :=
		 case
			(!go & next(go)) & mpk1 = low : low;
			(!go & next(go)) & mpk1 = med : med;
			(!go & next(go)) & mpk1 = high : high;
			TRUE : LS;
		 esac;

-----sur2-----
	init(sur2) := low;
	next(sur2) :=
		   case
			(!go & next(go))  & mpk1 = low : low;
			(!go & next(go))  & mpk1 = med : med;
			(!go & next(go))  & mpk1 = high : high;
			TRUE : sur2;
		   esac;

-----let23----- includes cell cycle regulation
	init(let23) :=
		    case
			v_let23 = ko : off;
			v_let23 = wt : low;
		    esac;
	next(let23) :=
		    case
                (!go & next(go)) & (cellcycle != G1phase & let23 = med & 
			(dep1 != off & (lst = high )) & IS = med) : low;
                (!go & next(go)) & let23 != off & IS = low1 : low1;
                (!go & next(go)) & ((cellcycle != G1phase & IS = high & let23 = high & ((dep1 != off & lst = high))) | 
			(cellcycle = G1phase & (let23 = low|let23 = low1) & IS = med) |
			(cellcycle != G1phase & (let23 = low|let23 = low1) & lst != high & IS = med)) : med;
			(!go & next(go)) & ((let23 != off & cellcycle = G1phase & IS = high)
			| (let23 != off & cellcycle != G1phase & (lst = med | lst = low | lst = off) & IS = high)) : high; 
			TRUE : let23;
		    esac;

-----sem5----- includes cell cycle regulation
	init(sem5) :=
		   case
			v_sem5 = ko : off;
			v_sem5 = rf : low;
			v_sem5 = wt : low;
		    esac;
	next(sem5) :=
		   case
                (!go & next(go)) & ((sem5 = med & (let23 = off | let23 = low)) |
			(cellcycle != G1phase & sem5 = med & let23 != high & lst = high)) : low;
                (!go & next(go)) & sem5 != off & let23 = low1 : low1;
			(!go & next(go)) & ((v_sem5 = rf & (cellcycle = G1phase & (sem5 = low|sem5 = low1)  & ((let23 = high )))
			| (cellcycle != G1phase & (sem5 = low|sem5 = low1) & ((let23 = high & (lst != high))))) |
			(v_sem5 != rf & ((cellcycle != G1phase & sem5 = high & let23 != high & (lst = med | lst = high)) |
			(cellcycle = G1phase & (sem5 = low|sem5 = low1)  & ((let23 = med )))
			| (cellcycle != G1phase & (sem5 = low|sem5 = low1)  & ((let23 = med & (lst != high))))))) : med;
			(!go & next(go)) & v_sem5 != rf & ((cellcycle = G1phase & sem5 != off & let23 = high)
			| (cellcycle != G1phase & sem5 != off 			& (lst != high) & let23 = high)) : high;
			TRUE : sem5;
		   esac;

-----let60----- includes cell cycle regulation
	init(let60) :=
		   case
			v_let60 = ko : off;
			v_let60 = wt : low;
			v_let60 = gf : med;
		    esac;
	next(let60) :=
		   case
                (!go & next(go)) & ((let60 = med & (sem5 = off | sem5 = low)) |
			(v_let60 != gf & let60 = med & sem5 = low1) |
			(cellcycle != G1phase & let60 = med & sem5 != high & lst = high)) : low;
			(!go & next(go)) & ((cellcycle != G1phase & let60 = high & sem5 != high & (lst = med | lst = high)) |			(cellcycle = G1phase & let60 = low & ((sem5 = med | (sem5 = low1 & v_let60 = gf))))
			| (cellcycle != G1phase & let60 = low & (((sem5 = med | (sem5 = low1 & v_let60 = gf)) & (lst != high))))) : med;
			(!go & next(go)) & ((cellcycle = G1phase & let60 != off & sem5 = high)
			| (cellcycle != G1phase & let60 != off & (lst != high) & sem5 = high)) : high;
			TRUE : let60;
		   esac;

-----mpk1----- includes cell cycle regulation
	init(mpk1) :=
		   case
			v_mpk1 = ko : off;
			v_mpk1 = wt : low;
		   esac;
	next(mpk1) :=
		   case
                (!go & next(go)) & ((mpk1 = med & (let60 = off | let60 = low)) |
			(cellcycle != G1phase & mpk1 = med & let60 != high & lst = high)) : low;
			(!go & next(go)) & ((cellcycle != G1phase & mpk1 = high & let60 != high & (lst = med | lst = high)) |
			(cellcycle = G1phase & mpk1 = low & ((let60 = med)))
			| (cellcycle != G1phase & mpk1 = low & ((let60 = med & (lst != high))))) : med;
			(!go & next(go)) & ((cellcycle = G1phase & mpk1 != off & let60 = high)
			| (cellcycle != G1phase & mpk1 != off & (lst != high) & let60 = high)) : high;
			TRUE : mpk1;
		   esac;

-----lin12----- includes cell cycle regulation
	init(lin12) :=
		   case
			v_lin12 = ko : off;
			v_lin12 = wt : low;
			v_lin12 = gf : high;
		   esac;
	next(lin12) :=
		    case
			(!go & next(go)) & cellcycle != G1phase & counter >= 1 & lin12 = med & 
			(sur2 = high | sur2 = med) : low;
                (!go & next(go)) & ((cellcycle = G1phase & lin12 = low 
			& (LSleft = med | LSright = med))
			|(cellcycle != G1phase & counter >= 1 & lin12 = low & (LSleft = med | LSright = med)
			& (sur2 = low | sur2 = off))
			| (lin12 = high & cellcycle != G1phase & counter >= 1 & (sur2 = high | sur2 = med))): med;
			(!go & next(go)) & ((cellcycle = G1phase & lin12 != off & (LSleft = high | LSright = high)) 
			| (cellcycle != G1phase & counter >= 1 & lin12 != off & (sur2 != high & sur2 != med) & (LSleft = high | LSright = high))): high;
			TRUE : lin12;
		    esac;

-----dep1----- 	
		init(dep1) :=
			   case
				v_dep1 = ko : off;
				v_dep1 = wt : low;
			   esac;
		next(dep1) :=
			   case
				(!go & next(go))  & dep1 != off & (sur2 != off & sur2 != low) : low;
				(!go & next(go)) & (( dep1 != off & (sur2 = off | sur2 = low))): med;
				TRUE : dep1;
			   esac;

-----fate-----
	init(fate) := af;
	next(fate) :=
		   case
			(!go & next(go)) & (fate = af &  
			((cellcycle = Sphase & mpk1 = high & (lin12 = off | lin12 = low | lin12 = med )) | 
			(cellcycle = Sphase & counter = 4 & mpk1 = med & (lin12 = off | lin12 = low)))) : primary;
			(!go & next(go)) & (fate = af & cellcycle = G2phase & mpk1 != high & 
			lin12 =high) : secondary;
			(!go & next(go)) & (fate = af & cellcycle = G2phase & (lin12 = off | lin12 = low | lin12 = med )
			& (mpk1 = off | mpk1 = low)): tertiary;
 			(!go & next(go)) & ((fate = af) & cellcycle = G2phase & ((lin12 = high & mpk1 = high) | 
			(lin12 = med & mpk1 = med))) : mixed;
			TRUE : fate;
		   esac;

-----cell cycle-----
	  init(cellcycle) := G1phase;
	  next(cellcycle) :=
	  		  case
				(!go & next(go)) & cellcycle = G1phase & time = 15 : Sphase;
				(!go & next(go)) & cellcycle = Sphase & counter > 10 : G2phase;
				TRUE : cellcycle;
			  esac;

-----counter-----
	init(counter) := 0;
	next(counter) :=
		      case
			(!go & next(go)) & cellcycle = Sphase & counter < 11 : counter+1;
			TRUE : counter;
		      esac;

----------------------------------------------------------------
"
let test3 = "MODULE Anchorcell(a_ac, a_lin3, a_lin15)

VAR
	lin3 : {off, med};
	lin15 : {off, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10,        t11, t12, t13, t14, t15, t16, on};
	ISd0 : {off, low1, low, med, high};
	ISd1 : {off, low1, low, med, high};
	ISd2 : {off, low1, low, med, high};
	ISd3 : {off, low1, low, med, high};

ASSIGN

-----lin3-----
	init(lin3) :=
		   case
			a_ac = ablated : off;
			a_ac = formed : med;
		   esac;
	next(lin3) := lin3;

-----lin15-----
	init(lin15) :=
		case
			a_lin15 = wt : on;
			a_lin15 = ko : off;
			a_lin15 = rf : off;
		esac;
	next(lin15) :=
		case
			lin15 = off : t1;
			lin15 = t1 : t2;
			lin15 = t2 : t3;
			lin15 = t3 : t4;
			lin15 = t4 : t5;
			lin15 = t5 : t6;
			lin15 = t6 : t7;
			lin15 = t7 : t8;
			lin15 = t8 : t9;
			lin15 = t9 : t10;
			lin15 = t10 : t11;
			lin15 = t11 : t12;
			lin15 = t12 : t13;
			lin15 = t13 : t14;
			lin15 = t14 : t15;
			lin15 = t15 : t16;
			TRUE : lin15;
		esac;	

-----IS----- 
INIT
	lin3 = med   -> (ISd0 = high &  ISd1 = med & ISd2 = low1 & ISd3 = low)
INIT	
	lin3 = off -> (ISd0 = low & ISd1 = low & ISd2 = low & ISd3 = low)
TRANS
	((next(ISd0) = ISd0 & next(ISd1) = ISd1 & next(ISd2) = ISd2 & next(ISd3) = ISd3 & lin15 != t16) | 
	(next(ISd0) = high & next(ISd1) = high & next(ISd2) = high & next(ISd3) = high & lin15 = t16 & a_lin15 != rf ) |
	(next(ISd0) = high & next(ISd1) = high & next(ISd2) = med & next(ISd3) = med & lin15 = t16 & a_lin15 = rf)) 
"

let test4 = "-- Model of the VPC system 
-- NuSMV 2.5.3
-- Tool download at http://nusmv.fbk.eu/

----------------------------------------------------------------

MODULE main

VAR
	mut : organiser;
	t   : timer;
	c   : clock;
	P3p : VPC(AC.ISd3, low, P4p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60,
	      mut.mpk1, mut.dep1, mut.lst, t.var1, c.time); 
	P4p : VPC(AC.ISd2, P3p.LS, P5p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var2, c.time);
	P5p : VPC(AC.ISd1, P4p.LS, P6p.LS, mut.lin12, 
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var3, c.time);
	P6p : VPC(AC.ISd0, P5p.LS, P7p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var4, c.time);
	P7p : VPC(AC.ISd1, P6p.LS, P8p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var5, c.time);
	P8p : VPC(AC.ISd2, P7p.LS, low, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst, t.var6, c.time);
	AC  : Anchorcell(mut.ac, mut.lin3, mut.lin15);

----------------------------------------------------------------

MODULE VPC(IS, LSleft, LSright, 	v_lin12, v_let23, v_sem5,
	      v_let60, v_mpk1, v_dep1, v_lst, go, time)

VAR
     lst   : {off, low, med, high};
	LS    : {low, med, high};
     sur2  : {off, low, med, high};
     let23 : {off, low, low1, med, high};
	sem5  : {off, low, low1, med, high};
	let60 : {off, low, med, high};
	mpk1  : {off, low, med, high};
	lin12 : {off, low, med, high};
	dep1  : {off, low, med};
	
	fate      : {af, primary, secondary, tertiary, mixed};
	cellcycle : {G1phase, Sphase, G2phase};
	counter   : 0..11;

ASSIGN

------lst-----
	init(lst) :=
		  case
			v_lst = ko : off;
			v_lst = wt : low;
		  esac;
	next(lst) :=
		  case
			(!go & next(go)) & lst != off & mpk1 = high : low; 
			(!go & next(go)) & ((lst = low & lin12 = med & mpk1 != high & mpk1 != med)
			| (lst = high & mpk1 = high)) : med; 
			(!go & next(go)) & (lst = low | lst = med) & 
			lin12 = high & mpk1 != high  : high;
			TRUE : lst; 
		  esac;

-----LS-----
	init(LS) := low;
	next(LS) :=
		 case
			(!go & next(go)) & mpk1 = low : low;
			(!go & next(go)) & mpk1 = med : med;
			(!go & next(go)) & mpk1 = high : high;
			TRUE : LS;
		 esac;

-----sur2-----
	init(sur2) := low;
	next(sur2) :=
		   case
			(!go & next(go))  & mpk1 = low : low;
			(!go & next(go))  & mpk1 = med : med;
			(!go & next(go))  & mpk1 = high : high;
			TRUE : sur2;
		   esac;

-----let23----- includes cell cycle regulation
	init(let23) :=
		    case
			v_let23 = ko : off;
			v_let23 = wt : low;
		    esac;
	next(let23) :=
		    case
                (!go & next(go)) & (cellcycle != G1phase & let23 = med & 
			(dep1 != off & (lst = high )) & IS = med) : low;
                (!go & next(go)) & let23 != off & IS = low1 : low1;
                (!go & next(go)) & ((cellcycle != G1phase & IS = high & let23 = high & ((dep1 != off & lst = high))) | 
			(cellcycle = G1phase & (let23 = low|let23 = low1) & IS = med) |
			(cellcycle != G1phase & (let23 = low|let23 = low1) & lst != high & IS = med)) : med;
			(!go & next(go)) & ((let23 != off & cellcycle = G1phase & IS = high)
			| (let23 != off & cellcycle != G1phase & (lst = med | lst = low | lst = off) & IS = high)) : high; 
			TRUE : let23;
		    esac;

-----sem5----- includes cell cycle regulation
	init(sem5) :=
		   case
			v_sem5 = ko : off;
			v_sem5 = rf : low;
			v_sem5 = wt : low;
		    esac;
	next(sem5) :=
		   case
                (!go & next(go)) & ((sem5 = med & (let23 = off | let23 = low)) |
			(cellcycle != G1phase & sem5 = med & let23 != high & lst = high)) : low;
                (!go & next(go)) & sem5 != off & let23 = low1 : low1;
			(!go & next(go)) & ((v_sem5 = rf & (cellcycle = G1phase & (sem5 = low|sem5 = low1)  & ((let23 = high )))
			| (cellcycle != G1phase & (sem5 = low|sem5 = low1) & ((let23 = high & (lst != high))))) |
			(v_sem5 != rf & ((cellcycle != G1phase & sem5 = high & let23 != high & (lst = med | lst = high)) |
			(cellcycle = G1phase & (sem5 = low|sem5 = low1)  & ((let23 = med )))
			| (cellcycle != G1phase & (sem5 = low|sem5 = low1)  & ((let23 = med & (lst != high))))))) : med;
			(!go & next(go)) & v_sem5 != rf & ((cellcycle = G1phase & sem5 != off & let23 = high)
			| (cellcycle != G1phase & sem5 != off 			& (lst != high) & let23 = high)) : high;
			TRUE : sem5;
		   esac;

-----let60----- includes cell cycle regulation
	init(let60) :=
		   case
			v_let60 = ko : off;
			v_let60 = wt : low;
			v_let60 = gf : med;
		    esac;
	next(let60) :=
		   case
                (!go & next(go)) & ((let60 = med & (sem5 = off | sem5 = low)) |
			(v_let60 != gf & let60 = med & sem5 = low1) |
			(cellcycle != G1phase & let60 = med & sem5 != high & lst = high)) : low;
			(!go & next(go)) & ((cellcycle != G1phase & let60 = high & sem5 != high & (lst = med | lst = high)) |			(cellcycle = G1phase & let60 = low & ((sem5 = med | (sem5 = low1 & v_let60 = gf))))
			| (cellcycle != G1phase & let60 = low & (((sem5 = med | (sem5 = low1 & v_let60 = gf)) & (lst != high))))) : med;
			(!go & next(go)) & ((cellcycle = G1phase & let60 != off & sem5 = high)
			| (cellcycle != G1phase & let60 != off & (lst != high) & sem5 = high)) : high;
			TRUE : let60;
		   esac;

-----mpk1----- includes cell cycle regulation
	init(mpk1) :=
		   case
			v_mpk1 = ko : off;
			v_mpk1 = wt : low;
		   esac;
	next(mpk1) :=
		   case
                (!go & next(go)) & ((mpk1 = med & (let60 = off | let60 = low)) |
			(cellcycle != G1phase & mpk1 = med & let60 != high & lst = high)) : low;
			(!go & next(go)) & ((cellcycle != G1phase & mpk1 = high & let60 != high & (lst = med | lst = high)) |
			(cellcycle = G1phase & mpk1 = low & ((let60 = med)))
			| (cellcycle != G1phase & mpk1 = low & ((let60 = med & (lst != high))))) : med;
			(!go & next(go)) & ((cellcycle = G1phase & mpk1 != off & let60 = high)
			| (cellcycle != G1phase & mpk1 != off & (lst != high) & let60 = high)) : high;
			TRUE : mpk1;
		   esac;

-----lin12----- includes cell cycle regulation
	init(lin12) :=
		   case
			v_lin12 = ko : off;
			v_lin12 = wt : low;
			v_lin12 = gf : high;
		   esac;
	next(lin12) :=
		    case
			(!go & next(go)) & cellcycle != G1phase & counter >= 1 & lin12 = med & 
			(sur2 = high | sur2 = med) : low;
                (!go & next(go)) & ((cellcycle = G1phase & lin12 = low 
			& (LSleft = med | LSright = med))
			|(cellcycle != G1phase & counter >= 1 & lin12 = low & (LSleft = med | LSright = med)
			& (sur2 = low | sur2 = off))
			| (lin12 = high & cellcycle != G1phase & counter >= 1 & (sur2 = high | sur2 = med))): med;
			(!go & next(go)) & ((cellcycle = G1phase & lin12 != off & (LSleft = high | LSright = high)) 
			| (cellcycle != G1phase & counter >= 1 & lin12 != off & (sur2 != high & sur2 != med) & (LSleft = high | LSright = high))): high;
			TRUE : lin12;
		    esac;

-----dep1----- 	
		init(dep1) :=
			   case
				v_dep1 = ko : off;
				v_dep1 = wt : low;
			   esac;
		next(dep1) :=
			   case
				(!go & next(go))  & dep1 != off & (sur2 != off & sur2 != low) : low;
				(!go & next(go)) & (( dep1 != off & (sur2 = off | sur2 = low))): med;
				TRUE : dep1;
			   esac;

-----fate-----
	init(fate) := af;
	next(fate) :=
		   case
			(!go & next(go)) & (fate = af &  
			((cellcycle = Sphase & mpk1 = high & (lin12 = off | lin12 = low | lin12 = med )) | 
			(cellcycle = Sphase & counter = 4 & mpk1 = med & (lin12 = off | lin12 = low)))) : primary;
			(!go & next(go)) & (fate = af & cellcycle = G2phase & mpk1 != high & 
			lin12 =high) : secondary;
			(!go & next(go)) & (fate = af & cellcycle = G2phase & (lin12 = off | lin12 = low | lin12 = med )
			& (mpk1 = off | mpk1 = low)): tertiary;
 			(!go & next(go)) & ((fate = af) & cellcycle = G2phase & ((lin12 = high & mpk1 = high) | 
			(lin12 = med & mpk1 = med))) : mixed;
			TRUE : fate;
		   esac;

-----cell cycle-----
	  init(cellcycle) := G1phase;
	  next(cellcycle) :=
	  		  case
				(!go & next(go)) & cellcycle = G1phase & time = 15 : Sphase;
				(!go & next(go)) & cellcycle = Sphase & counter > 10 : G2phase;
				TRUE : cellcycle;
			  esac;

-----counter-----
	init(counter) := 0;
	next(counter) :=
		      case
			(!go & next(go)) & cellcycle = Sphase & counter < 11 : counter+1;
			TRUE : counter;
		      esac;

----------------------------------------------------------------

MODULE Anchorcell(a_ac, a_lin3, a_lin15)

VAR
	lin3 : {off, med};
	lin15 : {off, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10,        t11, t12, t13, t14, t15, t16, on};
	ISd0 : {off, low1, low, med, high};
	ISd1 : {off, low1, low, med, high};
	ISd2 : {off, low1, low, med, high};
	ISd3 : {off, low1, low, med, high};

ASSIGN

-----lin3-----
	init(lin3) :=
		   case
			a_ac = ablated : off;
			a_ac = formed : med;
		   esac;
	next(lin3) := lin3;

-----lin15-----
	init(lin15) :=
		case
			a_lin15 = wt : on;
			a_lin15 = ko : off;
			a_lin15 = rf : off;
		esac;
	next(lin15) :=
		case
			lin15 = off : t1;
			lin15 = t1 : t2;
			lin15 = t2 : t3;
			lin15 = t3 : t4;
			lin15 = t4 : t5;
			lin15 = t5 : t6;
			lin15 = t6 : t7;
			lin15 = t7 : t8;
			lin15 = t8 : t9;
			lin15 = t9 : t10;
			lin15 = t10 : t11;
			lin15 = t11 : t12;
			lin15 = t12 : t13;
			lin15 = t13 : t14;
			lin15 = t14 : t15;
			lin15 = t15 : t16;
			TRUE : lin15;
		esac;	

-----IS----- 
INIT
	lin3 = med   -> (ISd0 = high &  ISd1 = med & ISd2 = low1 & ISd3 = low)
INIT	
	lin3 = off -> (ISd0 = low & ISd1 = low & ISd2 = low & ISd3 = low)
TRANS
	((next(ISd0) = ISd0 & next(ISd1) = ISd1 & next(ISd2) = ISd2 & next(ISd3) = ISd3 & lin15 != t16) | 
	(next(ISd0) = high & next(ISd1) = high & next(ISd2) = high & next(ISd3) = high & lin15 = t16 & a_lin15 != rf ) |
	(next(ISd0) = high & next(ISd1) = high & next(ISd2) = med & next(ISd3) = med & lin15 = t16 & a_lin15 = rf)) 

----------------------------------------------------------------

MODULE organiser

VAR
	ac    : {ablated, formed};
	lin3  : {ko, wt};
	lin15 : {ko, rf, wt};
	lin12 : {ko, wt, gf};
	let23 : {ko, wt};
	sem5  : {ko, rf, wt};
	let60 : {ko, wt, gf};
	mpk1  : {ko, wt};
	dep1  : {ko, wt};
	lst   : {ko, wt};

INIT
	(ac = formed ) &
	lin3 = wt &
	(lin15 = wt ) &
	(lin12 = wt ) &
	(let23 = wt ) &
	(sem5 = wt ) &
	(let60 = wt ) &
	(mpk1 = wt ) &
	(dep1 = wt ) &
	(lst = wt )

ASSIGN
	next(ac)    := ac;	
	next(lin3)  := lin3;
	next(lin15) := lin15;
	next(lin12) := lin12;
	next(let23) := let23;
	next(sem5)  := sem5;
	next(let60) := let60;
	next(mpk1)  := mpk1;
	next(dep1)  := dep1;
	next(lst)   := lst;

-----------------------------------------------------

MODULE timer

VAR
	var1  : boolean;
	var2  : boolean;
	var3  : boolean;
	var4  : boolean;
	var5  : boolean;
	var6  : boolean;
	reset : boolean;
INIT
	var1  = FALSE &
	var2  = FALSE &
	var3  = FALSE &
	var4  = FALSE &
	var5  = FALSE &
	var6  = FALSE &
	reset = FALSE
TRANS
	((!var1 & next(var1)) | (var1 & next(var1) & !reset) | (var1 & !next(var1) & reset)) & ((!var2 & next(var2)) | (var2 & next(var2) & !reset) | (var2 & !next(var2) & reset)) & ((!var3 & next(var3)) | (var3 & next(var3) & !reset) | (var3 & !next(var3) & reset)) & ((!var4 & next(var4)) | (var4 & next(var4) & !reset) | (var4 & !next(var4) & reset)) & ((!var5 & next(var5)) | (var5 & next(var5) & !reset) | (var5 & !next(var5) & reset)) & ((!var6 & next(var6)) | (var6 & next(var6) & !reset) | (var6 & !next(var6) & reset))
TRANS
	var1 & var2 & var3 & var4 & var5 & var6 -> reset

-----------------------------------------------------

MODULE clock

VAR
	time : 0..15;
ASSIGN
	init(time) := 0;
	next(time) := 
		   case
			time < 15 : time+1;
			TRUE : time;
		   esac;
"

let test5 = "
-- Model of the VPC system 
-- NuSMV 2.5.3
-- Tool download at http://nusmv.fbk.eu/

----------------------------------------------------------------

MODULE main

VAR
	mut : organiser;
	AC  : Anchorcell(mut.ac, mut.lin3, mut.lin15);
	P1p : VPC(AC.ISd3, low, P2p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60,
	      mut.mpk1, mut.dep1, mut.lst); 
	P2p : VPC(AC.ISd2, P1p.LS, P3p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst);
	P3p : VPC(AC.ISd1, P2p.LS, P4p.LS, mut.lin12, 
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst);
	P4p : VPC(AC.ISd0, P3p.LS, P5p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst);
	P5p : VPC(AC.ISd1, P4p.LS, P6p.LS, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst);
	P6p : VPC(AC.ISd2, P5p.LS, low, mut.lin12,
	      mut.let23, mut.sem5, mut.let60, 
	      mut.mpk1, mut.dep1, mut.lst);
		  

MODULE VPC(IS, LSleft, LSright, v_lin12, v_let23, v_sem5,
	      v_let60, v_mpk1, v_dep1, v_lst)

VAR
    lst   : {off, low, med, high};
	LS    : {low, med, high};
    sur2  : {off, low, med, high};
    let23 : {off, low, low1, med, high};
	sem5  : {off, low, low1, med, high};
	let60 : {off, low, med, high};
	mpk1  : {off, low, med, high};
	lin12 : {off, low, med, high};
	dep1  : {off, low, med};
	LSe   : {low, med, high};	
	fate      : {af, primary, secondary, tertiary, mixed};
	cellcycle : {G1phase , Sphase, G2phase};
	--counter   : 0..11;
	counter :0..20;

	
	INIT
	( LSe = low )
	TRANS
	( ( next(LSleft) = high | next(LSright) = high ) -> next(LSe) = high) &
	( ( next(LSleft) = med  | next(LSright) = med  ) & !( next(LSleft) = high | next(LSright) = high ) -> next(LSe) = med ) &
	( ( next(LSleft) = low  & next(LSright) = low  ) -> next(LSe) = low ) 


INIT
	( LS = low )
TRANS
	(next( mpk1 = off ) -> next( LS = low ) ) &
	(next( mpk1 = low ) -> next( LS = low ) ) &
	(next( mpk1 = med ) -> next( LS = med ) ) &
	(next( mpk1 = high ) -> next( LS = high ) )	
INIT
	( v_lst = ko & lst = off ) |  
	( v_lst = wt & lst = low ) 
TRANS
	( lst = off -> next(lst) =  lst ) &
	( ( lst != off ) & next(lin12) = off -> next(lst) = low ) & 
	--( ( lst != off ) & -> next(lst) = lst ) &
	--( next(mpk1) != low -> next(lst) =  lst ) &
	( ( lst != off ) & next(lin12) = high & ( next(mpk1) = low | next(mpk1) = off ) -> next(lst) = high ) &
	( ( lst != off ) & next(lin12) = med  & ( next(mpk1) = low | next(mpk1) = off ) -> next(lst) = med  ) &
	( ( lst != off ) & next(lin12) = low  & ( next(mpk1) = low | next(mpk1) = off ) -> next(lst) = low  ) &
	( ( lst != off ) & next(lin12) = high & ( next(mpk1) = med ) -> next(lst) = high ) &
        ( ( lst != off ) & next(lin12) = med  & ( next(mpk1) = med ) -> next(lst) = low  ) &
        ( ( lst != off ) & next(lin12) = low  & ( next(mpk1) = med ) -> next(lst) = low  ) &
	( ( lst != off ) & next(mpk1)  = high -> next(lst) =low )	
INIT
	( v_lin12 = ko & lin12 = off  ) |
	( v_lin12 = wt & lin12 = low  ) |
	( v_lin12 = gf & lin12 = low )
TRANS
	--( TRUE -> next(lin12) = lin12 )
	( lin12 =  off -> next(lin12) = off ) &
	( lin12 = high  & next(cellcycle) = G1phase -> next(lin12) = high ) &
	( lin12 != off  & next(cellcycle) = G1phase & next(LSe) = high  -> next(lin12) = high) &
	( lin12 =  med  & next(cellcycle) = G1phase & next(LSe) != high -> next(lin12) = med) &
	( lin12 =  low  & next(cellcycle) = G1phase & next(LSe) = low   -> next(lin12) = low) &	
	( lin12 =  low  & next(cellcycle) = G1phase & next(LSe) = med   -> next(lin12) = med) &
	( next(cellcycle) != G1phase & next(counter) = 0 -> lin12 = next(lin12) ) &

        ( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = low  &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = low ) &
	( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = med  &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = low ) &
	( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = high &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = med ) &
 	( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = med  ) &
        ( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = low  ) &
        ( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = med  ) &
        ( v_lin12 = wt & next(cellcycle) != G1phase & next(counter) >=1 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = med  ) &

	( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = low  &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = low ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = med  &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = low ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = high &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = med ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = med  ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = low  ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = med  ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=1 & next(counter) < 8 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = med  ) &

        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = low  &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = low ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = med  &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = low ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = high &  ( next(sur2) = med | next(sur2) = high ) -> next(lin12) = med ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = high  ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = high  ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = high  ) &
        ( v_lin12 = gf & next(cellcycle) != G1phase & next(counter) >=8 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = high  ) &


        ( next(cellcycle) != G1phase & next(counter) >=1 & lin12 = high & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = high -> next(lin12) = high ) &
	( next(cellcycle) != G1phase & next(counter) >=1 & lin12 = low  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = high -> next(lin12) = high ) &
	( next(cellcycle) != G1phase & next(counter) >=1 & lin12 = med  & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = high -> next(lin12) = high ) &
        ( next(cellcycle) != G1phase & next(counter) >=1 & lin12 = high & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = med  -> next(lin12) = high ) &
        ( next(cellcycle) != G1phase & next(counter) >=1 & lin12 = high & !( next(sur2) = med | next(sur2) = high ) & next(LSe) = low  -> next(lin12) = high )
	 	

	
ASSIGN

------lst-----
--	init(lst) :=
--		  case
--			v_lst = ko : off;
--			v_lst = wt : low;
--		  esac;
--	next(lst) :=
--		  case
--			 lst != off & mpk1 = high : low; 
--			 ((lst = low & lin12 = med & mpk1 != high & mpk1 != med)
--			| (lst = high & mpk1 = high)) : med; 
--			 (lst = low | lst = med) & 
--			lin12 = high & mpk1 != high  : high;
--			TRUE : lst; 
--		  esac;

-----LS-----
--	init(LS) := low;
--	next(LS) :=
--		 case
--			 mpk1 = low : low;
--			 mpk1 = med : med;
--			 mpk1 = high : high;
--			TRUE : LS;
--		 esac;

-----sur2-----
	init(sur2) := low;
	next(sur2) :=
		   case
			 mpk1 = low : low;
			mpk1 = med : med;
			 mpk1 = high : high;
			TRUE : sur2;
		   esac;

-----let23----- includes cell cycle regulation
	init(let23) :=
		    case
			v_let23 = ko : off;
			v_let23 = wt : low;
		    esac;
	next(let23) :=
		    case
             (cellcycle != G1phase & let23 = med & 
			(dep1 != off & (lst = high )) & IS = med) : low;
                 let23 != off & IS = low1 : low1;
                 ((cellcycle != G1phase & IS = high & let23 = high & ((dep1 != off & lst = high))) | 
			(cellcycle = G1phase & (let23 = low|let23 = low1) & IS = med) |
			(cellcycle != G1phase & (let23 = low|let23 = low1) & lst != high & IS = med)) : med;
			 ((let23 != off & cellcycle = G1phase & IS = high)
			| (let23 != off & cellcycle != G1phase & (lst = med | lst = low | lst = off) & IS = high)) : high; 
			TRUE : let23;
		    esac;

-----sem5----- includes cell cycle regulation
	init(sem5) :=
		   case
			v_sem5 = ko : off;
			v_sem5 = rf : low;
			v_sem5 = wt : low;
		    esac;
	next(sem5) :=
		   case
                 ((sem5 = med & (let23 = off | let23 = low)) |
			(cellcycle != G1phase & sem5 = med & let23 != high & lst = high)) : low;
                 sem5 != off & let23 = low1 : low1;
			 ((v_sem5 = rf & (cellcycle = G1phase & (sem5 = low|sem5 = low1)  & ((let23 = high )))
			| (cellcycle != G1phase & (sem5 = low|sem5 = low1) & ((let23 = high & (lst != high))))) |
			(v_sem5 != rf & ((cellcycle != G1phase & sem5 = high & let23 != high & (lst = med | lst = high)) |
			(cellcycle = G1phase & (sem5 = low|sem5 = low1)  & ((let23 = med )))
			| (cellcycle != G1phase & (sem5 = low|sem5 = low1)  & ((let23 = med & (lst != high))))))) : med;
			 v_sem5 != rf & ((cellcycle = G1phase & sem5 != off & let23 = high)
			| (cellcycle != G1phase & sem5 != off 			& (lst != high) & let23 = high)) : high;
			TRUE : sem5;
		   esac;

-----let60----- includes cell cycle regulation
	init(let60) :=
		   case
			v_let60 = ko : off;
			v_let60 = wt : low;
			v_let60 = gf : med;
		    esac;
	next(let60) :=
		   case
                ((let60 = med & (sem5 = off | sem5 = low)) |
			(v_let60 != gf & let60 = med & sem5 = low1) |
			(cellcycle != G1phase & let60 = med & sem5 != high & lst = high)) : low;
			 ((cellcycle != G1phase & let60 = high & sem5 != high & (lst = med | lst = high)) |			(cellcycle = G1phase & let60 = low & ((sem5 = med | (sem5 = low1 & v_let60 = gf))))
			| (cellcycle != G1phase & let60 = low & (((sem5 = med | (sem5 = low1 & v_let60 = gf)) & (lst != high))))) : med;
			 ((cellcycle = G1phase & let60 != off & sem5 = high)
			| (cellcycle != G1phase & let60 != off & (lst != high) & sem5 = high)) : high;
			TRUE : let60;
		   esac;

-----mpk1----- includes cell cycle regulation
	init(mpk1) :=
		   case
			v_mpk1 = ko : off;
			v_mpk1 = wt : low;
		   esac;
	next(mpk1) :=
		   case
                 ((mpk1 = med & (let60 = off | let60 = low)) |
			(cellcycle != G1phase & mpk1 = med & let60 != high & lst = high)) : low;
			((cellcycle != G1phase & mpk1 = high & let60 != high & (lst = med | lst = high)) |
			(cellcycle = G1phase & mpk1 = low & ((let60 = med)))
			| (cellcycle != G1phase & mpk1 = low & ((let60 = med & (lst != high))))) : med;
			((cellcycle = G1phase & mpk1 != off & let60 = high)
			| (cellcycle != G1phase & mpk1 != off & (lst != high) & let60 = high)) : high;
			TRUE : mpk1;
		   esac;

-----lin12----- includes cell cycle regulation
	init(lin12) :=
		   case
			v_lin12 = ko : off;
			v_lin12 = wt : low;
			v_lin12 = gf : high;
		   esac;
	next(lin12) :=
		    case
			 cellcycle != G1phase & counter >= 1 & lin12 = med & 
			(sur2 = high | sur2 = med) : low;
             ((cellcycle = G1phase & lin12 = low 
			& (LSleft = med | LSright = med))
			|(cellcycle != G1phase & counter >= 1 & lin12 = low & (LSleft = med | LSright = med)
			& (sur2 = low | sur2 = off))
			| (lin12 = high & cellcycle != G1phase & counter >= 1 & (sur2 = high | sur2 = med))): med;
			 ((cellcycle = G1phase & lin12 != off & (LSleft = high | LSright = high)) 
			| (cellcycle != G1phase & counter >= 1 & lin12 != off & (sur2 != high & sur2 != med) & (LSleft = high | LSright = high))): high;
			TRUE : lin12;
		    esac;

-----dep1----- 	
		init(dep1) :=
			   case
				v_dep1 = ko : off;
				v_dep1 = wt : low;
			   esac;
		next(dep1) :=
			   case
				 dep1 != off & (sur2 != off & sur2 != low) : low;
				 (( dep1 != off & (sur2 = off | sur2 = low))): med;
				TRUE : dep1;
			   esac;

-----fate-----
	init(fate) := af;
	next(fate) :=
		   case
			 (fate = af &  
			((cellcycle = Sphase & mpk1 = high & (lin12 = off | lin12 = low | lin12 = med )) | 
			(cellcycle = Sphase & counter = 4 & mpk1 = med & (lin12 = off | lin12 = low)))) : primary;
			 (fate = af & cellcycle = G2phase & mpk1 != high & 
			lin12 =high) : secondary;
			 (fate = af & cellcycle = G2phase & (lin12 = off | lin12 = low | lin12 = med )
			& (mpk1 = off | mpk1 = low)): tertiary;
 			 ((fate = af) & cellcycle = G2phase & ((lin12 = high & mpk1 = high) | 
			(lin12 = med & mpk1 = med))) : mixed;
			TRUE : fate;
		   esac;

-----cell cycle-----
	  init(cellcycle) := G1phase;
	  next(cellcycle) :=
	  		  case
				 cellcycle = G1phase : Sphase;
				 --& time = 15 : Sphase;
				 cellcycle = Sphase & counter > 10 : G2phase;
				TRUE : cellcycle;
			  esac;

-----counter-----
	init(counter) := 0;
	next(counter) :=
		      case
			 cellcycle = Sphase & counter < 11 : counter+1;
			TRUE : counter;
		      esac;

-----------------------


MODULE Anchorcell(a_ac, a_lin3, a_lin15)

VAR
	lin3 : {off, med};
	lin15 : {off, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10,        t11, t12, t13, t14, t15, t16, on};
	ISd0 : {off, low1, low, med, high};
	ISd1 : {off, low1, low, med, high};
	ISd2 : {off, low1, low, med, high};
	ISd3 : {off, low1, low, med, high};

ASSIGN

-----lin3-----
	init(lin3) :=
		   case
			a_ac = ablated : off;
			a_ac = formed : med;
		   esac;
	next(lin3) := 
		case
			TRUE : lin3;
		esac;

-----lin15-----
	init(lin15) :=
		case
			a_lin15 = wt : on;
			a_lin15 = ko : off;
			a_lin15 = rf : off;
		esac;
	next(lin15) :=
		case
			lin15 = off : t1;
			lin15 = t1 : t2;
			lin15 = t2 : t3;
			lin15 = t3 : t4;
			lin15 = t4 : t5;
			lin15 = t5 : t6;
			lin15 = t6 : t7;
			lin15 = t7 : t8;
			lin15 = t8 : t9;
			lin15 = t9 : t10;
			lin15 = t10 : t11;
			lin15 = t11 : t12;
			lin15 = t12 : t13;
			lin15 = t13 : t14;
			lin15 = t14 : t15;
			lin15 = t15 : t16;
			TRUE : lin15;
		esac;	

-----IS----- 
INIT
	lin3 = med   -> (ISd0 = high &  ISd1 = med & ISd2 = low1 & ISd3 = low)
INIT	
	lin3 = off -> (ISd0 = low & ISd1 = low & ISd2 = low & ISd3 = low)
TRANS
	((next(ISd0) = ISd0 & next(ISd1) = ISd1 & next(ISd2) = ISd2 & next(ISd3) = ISd3 & lin15 != t16) | 
	(next(ISd0) = high & next(ISd1) = high & next(ISd2) = high & next(ISd3) = high & lin15 = t16 & a_lin15 != rf ) |
	(next(ISd0) = high & next(ISd1) = high & next(ISd2) = med & next(ISd3) = med & lin15 = t16 & a_lin15 = rf)) 

----------------------------------------------------------------
MODULE organiser

VAR
	ac    : {ablated, formed};
	lin3  : {ko, wt};
	lin15 : {ko, rf, wt};
	lin12 : {ko, wt, gf};
	let23 : {ko, wt};
	sem5  : {ko, rf, wt};
	let60 : {ko, wt, gf};
	mpk1  : {ko, wt};
	dep1  : {ko, wt};
	lst   : {ko, wt};

INIT
	(ac = formed ) &
	(lin3 = wt) &
	(lin15 = wt ) &
	(lin12 = wt ) &
	(let23 = wt ) &
	(sem5 = wt ) &
	(let60 = wt ) &
	(mpk1 = wt ) &
	(dep1 = wt ) &
	(lst = wt )

TRANS
	(next(ac)=ac)&
	(next(lin3)=lin3)&
	(next(lin15)=lin15)&
	(next(lin12)=lin12)& 
	(next(let23)=let23)& 
	(next(sem5)=sem5)& 
	(next(let60)=let60)& 
	(next(mpk1)=mpk1)&
	(next(dep1)=dep1)& 
	(next(lst)=lst)
	"
let _ = testparser test1
let _ = testparser test2
let _ = testparser test3
let _ = testparser test4
let _ = testparser test5


