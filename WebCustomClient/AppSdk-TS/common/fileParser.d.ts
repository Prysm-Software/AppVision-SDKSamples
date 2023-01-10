import { EnvironmentModel } from "../data/environmentModels";
export declare abstract class FileParser {
    static parseWebEnvironment(file: string): EnvironmentModel;
    static parseClassicContext(file: string): EnvironmentModel;
    private static parseVideoLayout;
    private static parseClientLayout;
    private static parseContainerXml;
}
