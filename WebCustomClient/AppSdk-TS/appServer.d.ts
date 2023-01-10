import { VariableManager } from './managers/variableManager';
import { GroupManager } from './managers/groupManager';
import { OptionManager } from './managers/optionManager';
import { UserManager } from './managers/userManager';
import { FileManager } from './managers/fileManager';
import { EventManager } from './managers/eventManager';
import { AlarmManager } from './managers/alarmManager';
import { OperandManager } from './managers/operandManager';
import { RTManager } from './managers/rtManager';
import { ProtocolManager } from './managers/protocolManager';
import { Subject } from 'rxjs';
import { ScenarioManager } from './managers/scenarioManager';
import { AreaManager } from './managers/areaManager';
import { CommandManager } from './managers/commandManager';
import { VideoManager } from './managers/videoManager';
import { HttpService } from './services/httpService';
import { EnvironmentManager } from './managers/environmentManager';
import { CategoryManager } from './managers/categoryManager';
import { PeopleManager } from './managers/peopleManager';
import { DepartmentManager } from './managers/departmentManager';
import { AccessProfileManager } from './managers/accessProfileManager';
import { BadgeManager } from './managers/badgeManager';
import { RightManager } from './managers/rightManager';
import { BadgeTypeManager } from './managers/badgeTypeManager';
import { ZoneManager } from './managers/zoneManager';
import { PersonZoneManager } from './managers/personZoneManager';
import { ServerState } from './data/serverState';
import { HolidayManager } from './managers/holidayManager';
import { UserMessageManager } from './managers/userMessageManager';
import { VisitorManager } from './managers/visitorManager';
import { LicenseManager } from './managers/licenseManager';
import { ChatManager } from './managers/chatManager';
import { AppLoginOptions } from './appLoginOptions';
import { DeviceManager } from './managers/deviceManager';
import { RowCustomColorManager } from './managers/rowCustomColorManager';
import { OverlayManager } from './managers/overlayManager';
/**
 * Object to connect to an appvision server
 */
export declare class AppServer {
    private tempoKeepalive;
    private lastState;
    private keepAliveId;
    private connected;
    private httpOData;
    http: HttpService;
    /**
     * Error when parsing json response
     */
    error: Subject<any>;
    /**
     * Connection with appserver lost
     */
    connectionLost: Subject<any>;
    /**
     * New server state event
     */
    newState: Subject<ServerState>;
    alarmManager: AlarmManager;
    optionManager: OptionManager;
    userManager: UserManager;
    userMessageManager: UserMessageManager;
    variableManager: VariableManager;
    groupManager: GroupManager;
    areaManager: AreaManager;
    fileManager: FileManager;
    eventManager: EventManager;
    rtManager: RTManager;
    operandManager: OperandManager;
    protocolManager: ProtocolManager;
    scenarioManager: ScenarioManager;
    commandManager: CommandManager;
    videoManager: VideoManager;
    environmentManager: EnvironmentManager;
    licenseManager: LicenseManager;
    chatManager: ChatManager;
    deviceManager: DeviceManager;
    customColorManager: RowCustomColorManager;
    overlayManager: OverlayManager;
    peopleManager: PeopleManager;
    visitorManager: VisitorManager;
    categoryManager: CategoryManager;
    departmentManager: DepartmentManager;
    accessProfileManager: AccessProfileManager;
    badgesManager: BadgeManager;
    rightManager: RightManager;
    badgeTypeManager: BadgeTypeManager;
    zoneManager: ZoneManager;
    personZoneManager: PersonZoneManager;
    holidayManager: HolidayManager;
    constructor();
    /**
     * Current state of the server
     */
    get state(): ServerState;
    /**
     * Tell if the appServer object is connected or not
     */
    get isConnected(): boolean;
    /**
     * Login informations
     * @param options
     */
    login(options: AppLoginOptions): Promise<void>;
    /**
     * Logout
     */
    logout(): Promise<void>;
    private keepAlive;
    private handleNewEvents;
    private dispose;
}
