using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/*
 * 创建时间：2016-07-19
 * 创建人：刘自洋
 * 说明：此文件下包含所有微信实体类
 */
namespace FineUIMvc.EmptyProject.Models
{
    public class WeChatModel
    {
    }

    #region 系统返回信息
    /// <summary>
    /// 系统返回信息
    /// </summary>
    /// <typeparam name="T">指定返回类型</typeparam>
    public class wx_backdata<T>
    {
        /// <summary>
        /// 接口返回数据状态true成功|false失败
        /// </summary>
        public bool ResponseState;
        /// <summary>
        /// 接口返回正确数据
        /// </summary>
        public T ResponseData;
        /// <summary>
        /// 接口返回错误时间
        /// </summary>
        public wx_apperror ErrorData;
    }
    #endregion

    #region 微信接口返回错误类
    /// <summary>
    /// 微信接口返回错误类
    /// </summary>
    public class wx_apperror
    {
        /// <summary>
        /// 接口错误代码
        /// </summary>
        public string errcode;
        /// <summary>
        /// 接口错误消息
        /// </summary>
        public string errmsg;
    }
    #endregion

    #region 获取接口返回Token
    /// <summary>
    /// 获取接口返回凭证Token
    /// </summary>
    public class wx_access_token
    {
        /// <summary>
        /// 获取到的凭证
        /// </summary>
        public string access_token;
        /// <summary>
        /// 凭证有效时间，单位：秒
        /// </summary>
        public string expires_in;
    }

    #endregion

    #region  获取此用户OpenID
    /// <summary>
    /// 获取用户OpenID列表
    /// </summary>
    public class wx_openidlist
    {
        /// <summary>
        /// 关注该公众账号的总用户数
        /// </summary>
        public string total;
        /// <summary>
        /// 拉取的OPENID个数，最大值为10000
        /// </summary>
        public string count;
        /// <summary>
        /// 列表数据，OPENID的列表
        /// </summary>
        public data data;
        /// <summary>
        /// 拉取列表的最后一个用户的OPENID
        /// </summary>
        public string next_openid;
    }
    /// <summary>
    /// openid对象
    /// </summary>
    public class data
    {
        /// <summary>
        /// 用户标识
        /// </summary>
        public List<string> openid;
    }
    #endregion

    #region 获取用户个人信息
    public class wx_user_info
    {
        /// <summary>
        /// 是否订阅该公众号0没有关注|1已关注
        /// </summary>
        public string subscribe;
        /// <summary>
        /// 用户的标识，对当前公众号唯一
        /// </summary>
        public string openid;
        /// <summary>
        /// 用户的昵称
        /// </summary>
        public string nickname;
        /// <summary>
        /// 用户的性别0未知|1男性|2女性
        /// </summary>
        public string sex;
        /// <summary>
        /// 用户所在城市
        /// </summary>
        public string city;
        /// <summary>
        /// 用户所在国家
        /// </summary>
        public string country;
        /// <summary>
        /// 用户所在省份
        /// </summary>
        public string province;
        /// <summary>
        /// 用户的语言
        /// </summary>
        public string language;
        /// <summary>
        /// 用户头像,最后一个数值代表正方形头像大小（有0、46、64、96、132数值可选，0代表640*640正方形头像）
        /// </summary>
        public string headimgurl;
        /// <summary>
        /// 用户关注时间
        /// </summary>
        public string subscribe_time;
        /// <summary>
        /// 绑定到微信开放平台唯一标识
        /// </summary>
        public string unionid;
        /// <summary>
        /// 粉丝备注
        /// </summary>
        public string remark;
        /// <summary>
        /// 用户所在的分组ID（兼容旧的用户分组接口）
        /// </summary>
        public string groupid;
        /// <summary>
        /// 用户被打上的标签ID列表
        /// </summary>
        public List<string> tagid_list;
    }
    #endregion

    #region 二维码Ticket
    public class wx_ticket
    {
        /// <summary>
        /// 获取的二维码ticket，凭借此ticket可以在有效时间内换取二维码。
        /// </summary>
        public string ticket;

        /// <summary>
        /// 该二维码有效时间，以秒为单位。 最大不超过2592000（即30天）。
        /// </summary>
        public int expire_seconds;

        /// <summary>
        /// 二维码图片解析后的地址，开发者可根据该地址自行生成需要的二维码图片
        /// </summary>
        public string url;
    }

    #endregion

    #region OAuth2 Token
    public class wx_oauth2token
    {
        /// <summary>
        /// 网页授权接口调用凭证,注意：此access_token与基础支持的access_token不同
        /// </summary>
        public string access_token;

        /// <summary>
        /// access_token接口调用凭证超时时间，单位（秒）
        /// </summary>
        public int expires_in;

        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        public string refresh_token;

        /// <summary>
        /// 用户唯一标识，请注意，在未关注公众号时，用户访问公众号的网页，也会产生一个用户和公众号唯一的OpenID
        /// </summary>
        public string openid;

        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string scope;
    }

    #endregion
}
