export interface BMAModel {
    Name: string,
    variables: BMAModelVariable[]
}

export interface BMAModelVariable {
    Name: string
    Id: number
    RangeFrom: number
    RangeTo: number
}