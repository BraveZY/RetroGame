export const VIEW_WIDTH = 960;
export const VIEW_HEIGHT = 540;

export const palette = {
  paper: 0xfff6df,
  cream: 0xffedc2,
  ink: 0x2f2a23,
  muted: 0x766957,
  line: 0x3b342b,
  court: 0xf28a64,
  arcade: 0x2f93c5,
  tv: 0x77b96d,
  chalk: 0xf8f5e9,
  red: 0xd94f45,
  green: 0x49a968,
  blue: 0x2b75b8,
  yellow: 0xf6c453,
  shadow: 0x2d241b,
  white: 0xffffff,
};

export type ThemeId = "playground" | "arcade" | "fitness";
export type GameId = "wooden" | "basket" | "aerobics";
export type SettingsGroupId = "family" | "experience" | "privacy" | "account" | "legal" | "about";
export type AvatarId = "captain" | "flag" | "soda" | "whistle" | "tvStar" | "arcadeLight";
export type Screen =
  | "healthAdvice"
  | "login"
  | "phoneLogin"
  | "realNameAuth"
  | "home"
  | "theme"
  | "prepare"
  | "recognition"
  | "game"
  | "pause"
  | "result"
  | "reward"
  | "collection"
  | "help"
  | "settings";

export type Theme = {
  id: ThemeId;
  game: GameId;
  name: string;
  moment: string;
  line: string;
  objects: string;
  action: string;
  color: number;
  gameName: string;
  gameLine: string;
  goal: string;
  unlock: string;
  tags: string[];
};

export type AppState = {
  screen: Screen;
  selectedTheme: Theme;
  lightState: "green" | "red";
  selectedAvatar: AvatarId;
  pendingAvatar: AvatarId;
  avatarPickerOpen: boolean;
  agreementAccepted: boolean;
  agreementShakeOffset: number;
  activeInput: "phone" | "code" | "realName" | "idNumber" | null;
  phoneNumber: string;
  verificationCode: string;
  realName: string;
  idNumber: string;
  selectedSettingsGroup: SettingsGroupId;
  collectionScroll: number;
};

export type AppActions = {
  nav: (screen: Screen) => void;
  selectTheme: (theme: Theme) => void;
  toggleLight: () => void;
  requestAgreement: () => void;
  toggleAgreement: () => void;
  focusInput: (input: AppState["activeInput"]) => void;
  typeInput: (value: string) => void;
  backspaceInput: () => void;
  selectSettingsGroup: (group: SettingsGroupId) => void;
  scrollCollection: (deltaY: number) => void;
  openAvatarPicker: () => void;
  closeAvatarPicker: () => void;
  chooseAvatar: (avatar: AvatarId) => void;
  saveAvatar: () => void;
};

export type RenderContext = {
  state: AppState;
  actions: AppActions;
};

export const themes: Theme[] = [
  {
    id: "playground",
    game: "wooden",
    name: "童年操场",
    moment: "上午课间",
    line: "铃声一响，跑向操场。红灯停，绿灯跑。",
    objects: "粉笔线 / 跑道 / 小队旗",
    action: "跑 / 停 / 定住",
    color: palette.court,
    gameName: "操场木头人",
    gameLine: "老师看过来了，别动。",
    goal: "今日目标：定住 5 次",
    unlock: "可解锁：课间冲刺王",
    tags: ["木头人", "跳房子", "丢手绢", "跑停"],
  },
  {
    id: "arcade",
    game: "basket",
    name: "客厅街机",
    moment: "放学之后",
    line: "投币声响起，分数灯亮起来。今天冲一局投篮机。",
    objects: "灯牌 / 篮筐 / 投币按钮",
    action: "投篮 / 挥臂 / 冲分",
    color: palette.arcade,
    gameName: "客厅街机投篮机",
    gameLine: "举手投篮，连续命中进加分时间。",
    goal: "今日目标：连中 4 次",
    unlock: "可解锁：街机高分贴纸",
    tags: ["投篮机", "打地鼠", "拳击机", "反应"],
  },
  {
    id: "fitness",
    game: "aerobics",
    name: "家庭节拍馆",
    moment: "傍晚客厅",
    line: "打开电视，全家跟着节拍动起来。",
    objects: "电视框 / 节拍 / 节目单",
    action: "抬膝 / 摆臂 / 同步",
    color: palette.tv,
    gameName: "家庭节拍馆",
    gameLine: "抬膝、摆臂，跟着节拍完成动作。",
    goal: "今日目标：连续跟拍 20 秒",
    unlock: "可解锁：客厅节拍贴纸",
    tags: ["健美操", "广播体操", "拳击操", "跟拍"],
  },
];

export const avatarOptions: Array<{ id: AvatarId; label: string; mark: string; color: number }> = [
  { id: "captain", label: "小队长", mark: "队", color: palette.blue },
  { id: "flag", label: "小队旗", mark: "旗", color: palette.red },
  { id: "soda", label: "汽水贴纸", mark: "汽", color: palette.green },
  { id: "whistle", label: "口哨", mark: "哨", color: palette.yellow },
  { id: "tvStar", label: "电视星", mark: "星", color: palette.tv },
  { id: "arcadeLight", label: "投篮灯", mark: "灯", color: palette.arcade },
];

export function getAvatarOption(id: AvatarId) {
  return avatarOptions.find((option) => option.id === id) ?? avatarOptions[0];
}
