﻿module Adrien.Derivatives

open System.Numerics;

type numeric = float32
type vector = numeric[]
type tensor = Tensor<numeric>

(* Scalar or vector or tensor numeric type*)
type A = 
    | Scalar of numeric
    | Vector of vector 
    | Tensor of tensor
    | Derivative of x : A * dx: A * tag: uint32 //forward derivative consisting of primal function, tangent, tag  
    
    member a.Zero = 
        match a with
        | Scalar _ -> A.Scalar 0.0f
        | Vector v -> A.Vector (Array.zeroCreate v.Rank)
        | Tensor t -> A.Tensor (Tensor.CreateIdentity(t.Rank, true, 0.0f))
        | Derivative(x, _, _) -> x.Zero

    member a.Rank = 
        match a with
        | Scalar _ -> 1
        | Vector v -> v.Rank
        | Tensor t -> t.Rank

    member a.Primal =
        match a with
        | Scalar _ -> a
        | Vector _ -> a
        | Tensor _ -> a
        | Derivative(x, _, _) -> x
    
    member a.Tangent =
        match a with
        | Scalar _ -> a.Zero
        | Vector v -> a.Zero
        | Tensor _ -> a.Zero
        | Derivative(_, d, _) -> d

    //The derivative of f(g(x)) wrt x is f'(g(x)) * g'(x) or df/dg * dg/dx
    member inline a.UnaryOp (ff, fd, df) =
        match a with
        | Scalar x  -> Scalar(ff(x))
       
        | Derivative(x, dx, tag) -> Derivative(fd(x), df(fd(x), x, dx), tag)
    
    member inline a.BinaryOp(b, ff, fd, df_da, df_db, df_dab) =
        match a with
        | Scalar x ->
            match b with
            | Scalar y    -> Scalar(ff(x, y))
            | Derivative(y, dy, ytag) -> let cp = fd(a, y) in Derivative(cp, df_db(cp, y, dy), ytag)
         
        | Derivative(x, dx, xtag) ->
            match b with
            | Scalar _  -> let cp = fd(x, b) in Derivative(cp, df_da(cp, x, dx), xtag)
            | Derivative(y, dy, ytag) ->
                match compare xtag ytag with
                | 0                  -> let cp = fd(x, y) in Derivative(cp, df_dab(cp, x, dx, y, dy), xtag) // ai = bi
                | -1                 -> let cp = fd(a, y) in Derivative(cp, df_db(cp, y, dy), ytag) // ai < bi
                | _ -> let cp = fd(x, b) in Derivative(cp, df_da(cp, y, dy), ytag) // ai > bi
    
    

    static member (~-) (a:A) =
        let inline ff(a) = -a
        let inline fd(a) = -a
        let inline df(cp, ap, at) = -at
        a.UnaryOp(ff, fd, df)

    static member (+) (a:A, b:A) =
        let inline ff(a, b) = a + b
        let inline fd(a, b) = a + b
        let inline df_da(cp, ap, at) = at
        let inline df_db(cp, bp, bt) = bt
        let inline df_dab(cp, ap, at, bp, bt) = at + bt
        a.BinaryOp(b, ff, fd, df_da, df_db, df_dab)

    static member (*) (a:A, b:A) =
        let inline ff(a, b) = a * b
        let inline fd(a, b) = a * b
        let inline df_da(cp, ap, at) = at * b
        let inline df_db(cp, bp, bt) = a * bt
        let inline df_dab(cp, ap, at, bp, bt) = at * bp + ap * bt
        a.BinaryOp(b, ff, fd, df_da, df_db, df_dab)
    
    static member (-) (a:A, b:A) =
        let inline ff(a, b) = a - b
        let inline fd(a, b) = a - b
        let inline df_da(cp, ap, at) = at
        let inline df_db(cp, bp, bt) = -bt
        let inline df_dab(cp, ap, at, bp, bt) = at - bt
        a.BinaryOp(b, ff, fd, df_da, df_db, df_dab)

    static member Sin (a:A) =
        let inline ff(a) = sin a
        let inline fd(a) = sin a
        let inline df(cp:A, ap:A, at:A) = at * cos ap
        a.UnaryOp(ff, fd, df)

    static member Cos (a:A) =
        let inline ff(a) = cos a
        let inline fd(a) = cos a
        let inline df(cp:A, ap:A, at:A) = -at * sin ap
        a.UnaryOp(ff, fd, df)
    

    
        
    
and Op =
    // Scalar-valued operations
    | Add                  of A * A
    | Add_Cons             of A
    | Sub                  of A * A
    | Sub_Cons             of A
    | Mul                  of A * A
    | Mul_Cons             of A 
    | Pow                  of A * A
    | Pow_Cons             of A
    | Log                  of A
    | Log10                of A
    | Exp                  of A
    | Sin                  of A
    | Cos                  of A
    | Tan                  of A
    | Neg                  of A
    | Sqrt                 of A
    | Sinh                 of A
    | Cosh                 of A
    | Tanh                 of A
    | Asin                 of A
    | Acos                 of A
    | Atan                 of A
    | Abs                  of A
    | Sign                 of A
    | Aloor                of A
    | Ceil                 of A
    | Round                of A
    | ReLU                 of A
    | Sigmoid              of A

    // Vector-valued operations
     
    // Matrix-valued operations
    
    | Noop


let rec Zero f:A =
    match f with
        | Scalar _ -> A.Scalar 0.0f
        | Vector v -> A.Vector (Array.zeroCreate v.Rank)
        | Tensor t -> A.Tensor (Tensor.CreateIdentity(t.Rank, true, 0.0f))
        | Derivative(p, _, _) -> Zero p



