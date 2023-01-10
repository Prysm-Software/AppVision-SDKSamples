export declare type AppHandler = () => void;
export declare type AppReturnHandler<E> = () => E;
export declare type AppEventHandler<E> = (event: E) => void;
export declare type AppEventReturnHandler<E, T> = (event: E) => T;
