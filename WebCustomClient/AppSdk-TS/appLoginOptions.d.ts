/**
 * Login options
 */
export interface AppLoginOptions {
    /**
     * Server host
     */
    host: string;
    /**
     * Username
     */
    username: string;
    /**
     * Password
     */
    password: string;
    /**
     * Name of the product using the SDK
     */
    productName?: string;
    /**
     * Company name
     */
    productCompany?: string;
    /**
     * Application version
     */
    applicationVersion?: string;
    /**
     * Any others data to post
     */
    datas?: any;
}
